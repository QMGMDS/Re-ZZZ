using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

using GamePlay.Data;

namespace GamePlay.Editor
{
    internal sealed class CharacterActionLinkImportRow
    {
        internal CharacterActionLinkImportRow(
            int excelRowNumber,
            string fromActionId,
            string toActionId,
            CharacterIntention requiredIntention,
            CharacterFact requiredFact,
            int priority,
            float animationTransitionDurationSeconds)
        {
            ExcelRowNumber = excelRowNumber;
            FromActionId = fromActionId;
            ToActionId = toActionId;
            RequiredIntention = requiredIntention;
            RequiredFact = requiredFact;
            Priority = priority;
            AnimationTransitionDurationSeconds = animationTransitionDurationSeconds;
        }

        internal int ExcelRowNumber { get; }
        internal string FromActionId { get; }
        internal string ToActionId { get; }
        internal CharacterIntention RequiredIntention { get; }
        internal CharacterFact RequiredFact { get; }
        internal int Priority { get; }
        internal float AnimationTransitionDurationSeconds { get; }
    }

    internal static class CharacterActionLinkXlsxImporter
    {
        private const string FromActionHeader = "出边";
        private const string ToActionHeader = "去边";
        private const string IntentionHeader = "角色意图";
        private const string FactHeader = "角色事实";
        private const string PriorityHeader = "优先级";
        private const string TransitionHeader = "动画过渡";

        private const string SpreadsheetNamespaceUri =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelationshipNamespaceUri =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string PackageRelationshipNamespaceUri =
            "http://schemas.openxmlformats.org/package/2006/relationships";

        private static readonly XNamespace s_spreadsheetNamespace = SpreadsheetNamespaceUri;
        private static readonly XNamespace s_relationshipNamespace = RelationshipNamespaceUri;
        private static readonly XNamespace s_packageRelationshipNamespace =
            PackageRelationshipNamespaceUri;

        internal static IReadOnlyList<CharacterActionLinkImportRow> ReadAndValidate(
            string xlsxPath,
            CharacterActionSetAsset actionSet)
        {
            if (actionSet == null)
            {
                throw new ArgumentNullException(nameof(actionSet));
            }

            if (string.IsNullOrWhiteSpace(xlsxPath))
            {
                throw new ArgumentException("XLSX 文件路径不能为空", nameof(xlsxPath));
            }

            if (!File.Exists(xlsxPath))
            {
                throw new FileNotFoundException("找不到 XLSX 文件", xlsxPath);
            }

            if (!xlsxPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("请选择 XLSX 文件", nameof(xlsxPath));
            }

            HashSet<string> actionIds = BuildActionIds(actionSet);
            List<XlsxRow> worksheetRows = ReadFirstWorksheet(xlsxPath);
            int headerRowIndex = FindHeaderRow(worksheetRows);

            if (headerRowIndex < 0)
            {
                throw new InvalidOperationException("XLSX 第一张工作表中没有找到表头");
            }

            Dictionary<string, int> headerIndexes = BuildHeaderIndexes(
                worksheetRows[headerRowIndex]);
            List<CharacterActionLinkImportRow> importedRows =
                new List<CharacterActionLinkImportRow>();
            HashSet<string> linkKeys = new HashSet<string>(StringComparer.Ordinal);

            for (int rowIndex = headerRowIndex + 1; rowIndex < worksheetRows.Count; rowIndex++)
            {
                XlsxRow worksheetRow = worksheetRows[rowIndex];
                if (IsBlankRow(worksheetRow))
                {
                    continue;
                }

                CharacterActionLinkImportRow importedRow = ParseRow(
                    worksheetRow,
                    headerIndexes);

                ValidateActionReferences(importedRow, actionIds);

                string linkKey = importedRow.FromActionId + "\u001F" + importedRow.ToActionId;
                if (!linkKeys.Add(linkKey))
                {
                    throw CreateRowFormatException(
                        importedRow.ExcelRowNumber,
                        FromActionHeader,
                        "和去边组合重复");
                }

                importedRows.Add(importedRow);
            }

            return importedRows;
        }

        private static HashSet<string> BuildActionIds(CharacterActionSetAsset actionSet)
        {
            IReadOnlyList<CharacterActionAsset> actions = actionSet.Actions;
            if (actions == null)
            {
                throw new InvalidOperationException("动作资产集合没有动作列表");
            }

            if (actions.Count == 0)
            {
                throw new InvalidOperationException("动作资产集合不能为空动作列表");
            }

            HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < actions.Count; index++)
            {
                CharacterActionAsset action = actions[index];
                if (action == null)
                {
                    throw new InvalidOperationException(
                        $"动作资产集合第 {index} 项为空引用");
                }

                if (string.IsNullOrWhiteSpace(action.Id))
                {
                    throw new InvalidOperationException(
                        $"动作资产 {action.name} 的 Id 为空");
                }

                if (!actionIds.Add(action.Id))
                {
                    throw new InvalidOperationException(
                        $"动作资产 Id 重复：{action.Id}");
                }
            }

            return actionIds;
        }

        private static Dictionary<string, int> BuildHeaderIndexes(XlsxRow headerRow)
        {
            Dictionary<string, int> headerIndexes =
                new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < headerRow.Values.Count; index++)
            {
                string header = NormalizeCellValue(headerRow.Values[index]);
                if (header.Length == 0)
                {
                    continue;
                }

                if (!headerIndexes.TryAdd(header, index))
                {
                    throw new InvalidOperationException(
                        $"第 {headerRow.RowNumber} 行的表头重复：{header}");
                }
            }

            RequireHeader(headerIndexes, FromActionHeader, headerRow.RowNumber);
            RequireHeader(headerIndexes, ToActionHeader, headerRow.RowNumber);
            RequireHeader(headerIndexes, IntentionHeader, headerRow.RowNumber);
            RequireHeader(headerIndexes, FactHeader, headerRow.RowNumber);
            RequireHeader(headerIndexes, PriorityHeader, headerRow.RowNumber);
            RequireHeader(headerIndexes, TransitionHeader, headerRow.RowNumber);
            return headerIndexes;
        }

        private static void RequireHeader(
            IReadOnlyDictionary<string, int> headerIndexes,
            string header,
            int rowNumber)
        {
            if (!headerIndexes.ContainsKey(header))
            {
                throw new InvalidOperationException(
                    $"第 {rowNumber} 行缺少表头：{header}");
            }
        }

        private static CharacterActionLinkImportRow ParseRow(
            XlsxRow worksheetRow,
            IReadOnlyDictionary<string, int> headerIndexes)
        {
            string fromActionId = ReadRequiredCell(
                worksheetRow,
                headerIndexes[FromActionHeader],
                FromActionHeader);
            string toActionId = ReadRequiredCell(
                worksheetRow,
                headerIndexes[ToActionHeader],
                ToActionHeader);
            string intentionText = ReadRequiredCell(
                worksheetRow,
                headerIndexes[IntentionHeader],
                IntentionHeader);
            string factText = ReadRequiredCell(
                worksheetRow,
                headerIndexes[FactHeader],
                FactHeader);
            string priorityText = ReadRequiredCell(
                worksheetRow,
                headerIndexes[PriorityHeader],
                PriorityHeader);
            string transitionText = ReadRequiredCell(
                worksheetRow,
                headerIndexes[TransitionHeader],
                TransitionHeader);

            CharacterIntention requiredIntention = ParseIntention(
                intentionText,
                worksheetRow.RowNumber);
            CharacterFact requiredFact = ParseFact(
                factText,
                worksheetRow.RowNumber);
            int priority = ParsePriority(priorityText, worksheetRow.RowNumber);
            float transitionDuration = ParseTransitionDuration(
                transitionText,
                worksheetRow.RowNumber);

            return new CharacterActionLinkImportRow(
                worksheetRow.RowNumber,
                fromActionId,
                toActionId,
                requiredIntention,
                requiredFact,
                priority,
                transitionDuration);
        }

        private static string ReadRequiredCell(
            XlsxRow worksheetRow,
            int columnIndex,
            string columnName)
        {
            string value = columnIndex < worksheetRow.Values.Count
                ? NormalizeCellValue(worksheetRow.Values[columnIndex])
                : string.Empty;

            if (value.Length == 0)
            {
                throw CreateRowFormatException(
                    worksheetRow.RowNumber,
                    columnName,
                    "不能为空");
            }

            return value;
        }

        private static CharacterIntention ParseIntention(string value, int rowNumber)
        {
            ParsedConditionPair pair = ParseConditionPair(
                value,
                rowNumber,
                IntentionHeader,
                "Attack",
                "Move");

            return new CharacterIntention(pair.First, pair.Second);
        }

        private static CharacterFact ParseFact(string value, int rowNumber)
        {
            ParsedConditionPair pair = ParseConditionPair(
                value,
                rowNumber,
                FactHeader,
                "Death",
                "LogicalProgress");

            return new CharacterFact(pair.First, pair.Second);
        }

        private static ParsedConditionPair ParseConditionPair(
            string value,
            int rowNumber,
            string columnName,
            string firstName,
            string secondName)
        {
            string[] tokens = value.Split('+');
            if (tokens.Length != 2)
            {
                throw CreateRowFormatException(
                    rowNumber,
                    columnName,
                    $"必须使用 {firstName} 和 {secondName} 两个条件");
            }

            Dictionary<string, Trilean> parsedValues =
                new Dictionary<string, Trilean>(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index].Trim();
                int separatorIndex = token.IndexOf('_');

                if (separatorIndex <= 0 || separatorIndex >= token.Length - 1)
                {
                    throw CreateRowFormatException(
                        rowNumber,
                        columnName,
                        $"条件格式错误：{token}");
                }

                string marker = token.Substring(0, separatorIndex).Trim();
                string fieldName = token.Substring(separatorIndex + 1).Trim();
                if (!string.Equals(fieldName, firstName, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fieldName, secondName, StringComparison.OrdinalIgnoreCase))
                {
                    throw CreateRowFormatException(
                        rowNumber,
                        columnName,
                        $"未知条件字段：{fieldName}");
                }

                if (!parsedValues.TryAdd(fieldName, ParseTrilean(marker, rowNumber, columnName)))
                {
                    throw CreateRowFormatException(
                        rowNumber,
                        columnName,
                        $"条件字段重复：{fieldName}");
                }
            }

            if (!parsedValues.TryGetValue(firstName, out Trilean firstValue) ||
                !parsedValues.TryGetValue(secondName, out Trilean secondValue))
            {
                throw CreateRowFormatException(
                    rowNumber,
                    columnName,
                    $"必须包含 {firstName} 和 {secondName}");
            }

            return new ParsedConditionPair(firstValue, secondValue);
        }

        private static Trilean ParseTrilean(
            string marker,
            int rowNumber,
            string columnName)
        {
            switch (marker.ToUpperInvariant())
            {
                case "F":
                    return Trilean.False;
                case "T":
                    return Trilean.True;
                case "D":
                    return Trilean.DontCare;
                default:
                    throw CreateRowFormatException(
                        rowNumber,
                        columnName,
                        $"未知条件标记：{marker}");
            }
        }

        private static int ParsePriority(string value, int rowNumber)
        {
            if (!int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int priority))
            {
                throw CreateRowFormatException(
                    rowNumber,
                    PriorityHeader,
                    "必须是整数");
            }

            return priority;
        }

        private static float ParseTransitionDuration(string value, int rowNumber)
        {
            if (!float.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float duration) ||
                float.IsNaN(duration) ||
                float.IsInfinity(duration) ||
                duration < 0f)
            {
                throw CreateRowFormatException(
                    rowNumber,
                    TransitionHeader,
                    "必须是大于等于 0 的数字");
            }

            return duration;
        }

        private static void ValidateActionReferences(
            CharacterActionLinkImportRow importedRow,
            ISet<string> actionIds)
        {
            if (!actionIds.Contains(importedRow.FromActionId))
            {
                throw CreateRowFormatException(
                    importedRow.ExcelRowNumber,
                    FromActionHeader,
                    $"动作 Id 不存在：{importedRow.FromActionId}");
            }

            if (!actionIds.Contains(importedRow.ToActionId))
            {
                throw CreateRowFormatException(
                    importedRow.ExcelRowNumber,
                    ToActionHeader,
                    $"动作 Id 不存在：{importedRow.ToActionId}");
            }
        }

        private static FormatException CreateRowFormatException(
            int rowNumber,
            string columnName,
            string reason)
        {
            return new FormatException($"第 {rowNumber} 行的{columnName}{reason}");
        }

        private static int FindHeaderRow(IReadOnlyList<XlsxRow> worksheetRows)
        {
            for (int index = 0; index < worksheetRows.Count; index++)
            {
                if (!IsBlankRow(worksheetRows[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool IsBlankRow(XlsxRow row)
        {
            for (int index = 0; index < row.Values.Count; index++)
            {
                if (NormalizeCellValue(row.Values[index]).Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizeCellValue(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim().TrimStart('\uFEFF');
        }

        private static List<XlsxRow> ReadFirstWorksheet(string xlsxPath)
        {
            using (FileStream fileStream = File.OpenRead(xlsxPath))
            using (ZipArchive archive = new ZipArchive(
                       fileStream,
                       ZipArchiveMode.Read,
                       false))
            {
                ZipArchiveEntry workbookEntry = GetRequiredEntry(
                    archive,
                    "xl/workbook.xml",
                    "工作簿数据");
                XDocument workbookDocument = LoadDocument(workbookEntry);
                XElement workbookRoot = RequireRoot(workbookDocument, "工作簿数据");
                XElement sheetsElement = RequireElement(
                    workbookRoot,
                    s_spreadsheetNamespace + "sheets",
                    "工作表列表");
                XElement firstSheet = GetFirstElement(
                    sheetsElement,
                    s_spreadsheetNamespace + "sheet",
                    "工作表");

                XAttribute relationshipIdAttribute = firstSheet.Attribute(
                    s_relationshipNamespace + "id");
                string relationshipId = relationshipIdAttribute == null
                    ? string.Empty
                    : relationshipIdAttribute.Value;

                if (relationshipId.Length == 0)
                {
                    throw new InvalidOperationException("第一张工作表缺少关联信息");
                }

                ZipArchiveEntry relationshipsEntry = GetRequiredEntry(
                    archive,
                    "xl/_rels/workbook.xml.rels",
                    "工作簿关联数据");
                XDocument relationshipsDocument = LoadDocument(relationshipsEntry);
                XElement relationshipsRoot = RequireRoot(
                    relationshipsDocument,
                    "工作簿关联数据");
                string worksheetPath = FindWorksheetPath(
                    relationshipsRoot,
                    relationshipId);
                ZipArchiveEntry worksheetEntry = GetRequiredEntry(
                    archive,
                    worksheetPath,
                    "第一张工作表数据");
                List<string> sharedStrings = ReadSharedStrings(archive);
                return ReadWorksheetRows(worksheetEntry, sharedStrings);
            }
        }

        private static ZipArchiveEntry GetRequiredEntry(
            ZipArchive archive,
            string path,
            string description)
        {
            ZipArchiveEntry entry = archive.GetEntry(path);
            if (entry == null)
            {
                throw new InvalidOperationException($"XLSX 缺少{description}");
            }

            return entry;
        }

        private static XDocument LoadDocument(ZipArchiveEntry entry)
        {
            using (Stream stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }

        private static XElement RequireRoot(XDocument document, string description)
        {
            XElement root = document.Root;
            if (root == null)
            {
                throw new InvalidOperationException($"XLSX 缺少{description}根节点");
            }

            return root;
        }

        private static XElement RequireElement(
            XElement parent,
            XName elementName,
            string description)
        {
            XElement element = parent.Element(elementName);
            if (element == null)
            {
                throw new InvalidOperationException($"XLSX 缺少{description}");
            }

            return element;
        }

        private static XElement GetFirstElement(
            XElement parent,
            XName elementName,
            string description)
        {
            foreach (XElement element in parent.Elements(elementName))
            {
                return element;
            }

            throw new InvalidOperationException($"XLSX 没有{description}");
        }

        private static string FindWorksheetPath(
            XElement relationshipsRoot,
            string relationshipId)
        {
            foreach (XElement relationship in relationshipsRoot.Elements(
                         s_packageRelationshipNamespace + "Relationship"))
            {
                XAttribute idAttribute = relationship.Attribute("Id");
                if (idAttribute == null || idAttribute.Value != relationshipId)
                {
                    continue;
                }

                XAttribute targetAttribute = relationship.Attribute("Target");
                string target = targetAttribute == null
                    ? string.Empty
                    : targetAttribute.Value;

                if (target.Length == 0)
                {
                    break;
                }

                return ResolveZipPath("xl/workbook.xml", target);
            }

            throw new InvalidOperationException("第一张工作表的关联目标无效");
        }

        private static string ResolveZipPath(string sourcePath, string targetPath)
        {
            string normalizedTarget = targetPath.Replace('\\', '/');
            string combinedPath;

            if (normalizedTarget.StartsWith("/", StringComparison.Ordinal))
            {
                combinedPath = normalizedTarget.Substring(1);
            }
            else
            {
                int separatorIndex = sourcePath.LastIndexOf('/');
                string sourceDirectory = separatorIndex >= 0
                    ? sourcePath.Substring(0, separatorIndex + 1)
                    : string.Empty;
                combinedPath = sourceDirectory + normalizedTarget;
            }

            string[] pathParts = combinedPath.Split('/');
            List<string> normalizedParts = new List<string>(pathParts.Length);

            for (int index = 0; index < pathParts.Length; index++)
            {
                string pathPart = pathParts[index];
                if (pathPart.Length == 0 || pathPart == ".")
                {
                    continue;
                }

                if (pathPart == "..")
                {
                    if (normalizedParts.Count > 0)
                    {
                        normalizedParts.RemoveAt(normalizedParts.Count - 1);
                    }

                    continue;
                }

                normalizedParts.Add(pathPart);
            }

            return string.Join("/", normalizedParts);
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
            List<string> sharedStrings = new List<string>();
            if (sharedStringsEntry == null)
            {
                return sharedStrings;
            }

            XDocument sharedStringsDocument = LoadDocument(sharedStringsEntry);
            XElement root = RequireRoot(sharedStringsDocument, "共享字符串数据");

            foreach (XElement item in root.Elements(s_spreadsheetNamespace + "si"))
            {
                sharedStrings.Add(ReadRichText(item));
            }

            return sharedStrings;
        }

        private static List<XlsxRow> ReadWorksheetRows(
            ZipArchiveEntry worksheetEntry,
            IReadOnlyList<string> sharedStrings)
        {
            XDocument worksheetDocument = LoadDocument(worksheetEntry);
            XElement worksheetRoot = RequireRoot(worksheetDocument, "工作表数据");
            XElement sheetData = RequireElement(
                worksheetRoot,
                s_spreadsheetNamespace + "sheetData",
                "工作表行数据");
            List<XlsxRow> rows = new List<XlsxRow>();
            int fallbackRowNumber = 1;

            foreach (XElement rowElement in sheetData.Elements(s_spreadsheetNamespace + "row"))
            {
                XAttribute rowNumberAttribute = rowElement.Attribute("r");
                int rowNumber = fallbackRowNumber;
                if (rowNumberAttribute != null &&
                    int.TryParse(
                        rowNumberAttribute.Value,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int parsedRowNumber) &&
                    parsedRowNumber > 0)
                {
                    rowNumber = parsedRowNumber;
                }

                List<string> values = new List<string>();
                int nextColumnIndex = 0;

                foreach (XElement cell in rowElement.Elements(s_spreadsheetNamespace + "c"))
                {
                    XAttribute cellReferenceAttribute = cell.Attribute("r");
                    string cellReference = cellReferenceAttribute == null
                        ? string.Empty
                        : cellReferenceAttribute.Value;
                    int columnIndex = cellReference.Length == 0
                        ? nextColumnIndex
                        : ParseColumnIndex(cellReference);

                    while (values.Count <= columnIndex)
                    {
                        values.Add(string.Empty);
                    }

                    values[columnIndex] = ReadCellValue(cell, sharedStrings);
                    nextColumnIndex = columnIndex + 1;
                }

                rows.Add(new XlsxRow(rowNumber, values));
                fallbackRowNumber = rowNumber + 1;
            }

            return rows;
        }

        private static int ParseColumnIndex(string cellReference)
        {
            int column = 0;
            int letterCount = 0;

            for (int index = 0; index < cellReference.Length; index++)
            {
                char character = cellReference[index];
                if (character >= 'a' && character <= 'z')
                {
                    character = char.ToUpperInvariant(character);
                }

                if (character < 'A' || character > 'Z')
                {
                    break;
                }

                column = column * 26 + character - 'A' + 1;
                letterCount++;
            }

            if (letterCount == 0)
            {
                throw new InvalidOperationException($"XLSX 单元格引用无效：{cellReference}");
            }

            return column - 1;
        }

        private static string ReadCellValue(
            XElement cell,
            IReadOnlyList<string> sharedStrings)
        {
            XAttribute typeAttribute = cell.Attribute("t");
            string cellType = typeAttribute == null ? string.Empty : typeAttribute.Value;

            if (string.Equals(cellType, "inlineStr", StringComparison.Ordinal))
            {
                XElement inlineString = cell.Element(s_spreadsheetNamespace + "is");
                return inlineString == null ? string.Empty : ReadRichText(inlineString);
            }

            XElement valueElement = cell.Element(s_spreadsheetNamespace + "v");
            if (valueElement == null)
            {
                return string.Empty;
            }

            string value = valueElement.Value;
            if (string.Equals(cellType, "s", StringComparison.Ordinal))
            {
                if (!int.TryParse(
                        value,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int sharedStringIndex) ||
                    sharedStringIndex < 0 ||
                    sharedStringIndex >= sharedStrings.Count)
                {
                    throw new InvalidOperationException("XLSX 共享字符串索引无效");
                }

                return sharedStrings[sharedStringIndex];
            }

            if (string.Equals(cellType, "b", StringComparison.Ordinal))
            {
                return value == "1" ? "True" : "False";
            }

            return value;
        }

        private static string ReadRichText(XElement container)
        {
            StringBuilder builder = new StringBuilder();
            foreach (XElement textElement in container.Descendants(s_spreadsheetNamespace + "t"))
            {
                builder.Append(textElement.Value);
            }

            return builder.ToString();
        }

        private readonly struct ParsedConditionPair
        {
            internal ParsedConditionPair(Trilean first, Trilean second)
            {
                First = first;
                Second = second;
            }

            internal Trilean First { get; }
            internal Trilean Second { get; }
        }

        private sealed class XlsxRow
        {
            internal XlsxRow(int rowNumber, List<string> values)
            {
                RowNumber = rowNumber;
                Values = values;
            }

            internal int RowNumber { get; }
            internal List<string> Values { get; }
        }
    }
}
