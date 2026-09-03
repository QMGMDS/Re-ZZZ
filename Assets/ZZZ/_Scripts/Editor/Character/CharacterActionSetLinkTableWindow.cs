using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using UnityEditor;
using UnityEngine;

using GamePlay.Character;
using GamePlay.Definition;

namespace Editor
{
    internal sealed class CharacterActionSetLinkTableWindow : EditorWindow
    {
        private const int HeaderColumnCount = 17;
        private const int IntentionColumnStart = 7;
        private const int FactColumnStart = 13;

        private static readonly string[] TableHeaders =
        {
            "出边",
            "去边",
            "优先级",
            "打断窗口起点",
            "打断窗口终点",
            "动画过渡",
            string.Empty,
            "Move",
            "Evade",
            "Attack",
            "Skill",
            "Ultimate",
            string.Empty,
            "Death",
            "Hit",
            "SwitchIn",
            "SwitchOut"
        };

        private static readonly string[] IntentionFieldNames =
        {
            "_move",
            "_evade",
            "_attack",
            "_skill",
            "_ultimate"
        };

        private static readonly string[] FactFieldNames =
        {
            "_death",
            "_hit",
            "_switchIn",
            "_switchOut"
        };

        private static readonly XNamespace SpreadsheetNamespace =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace OfficeRelationshipsNamespace =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRelationshipsNamespace =
            "http://schemas.openxmlformats.org/package/2006/relationships";

        [SerializeField]
        private CharacterActionSetAsset _characterActionSetAsset;

        private string _statusMessage = string.Empty;
        private MessageType _statusMessageType = MessageType.Info;

        [MenuItem("Tools/ZZZ/Character/动作转移规则导表")]
        private static void Open()
        {
            GetWindow<CharacterActionSetLinkTableWindow>("动作转移规则导表");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("动作转移规则导表", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            _characterActionSetAsset = (CharacterActionSetAsset)EditorGUILayout.ObjectField(
                "动作资产集合",
                _characterActionSetAsset,
                typeof(CharacterActionSetAsset),
                false);

            int linkCount = _characterActionSetAsset == null
                ? 0
                : _characterActionSetAsset.Links.Count;
            EditorGUILayout.LabelField("当前规则数量", linkCount.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.HelpBox(
                "仅支持严格格式的 Excel 工作簿 第一张工作表第一行必须使用规定表头 空白行会被忽略 三态字段使用 F T D",
                MessageType.Info);

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("导入动作转移规则表", GUILayout.Height(28f)))
            {
                ImportTable();
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox(_statusMessage, _statusMessageType);
            }
        }

        private void ImportTable()
        {
            if (_characterActionSetAsset == null)
            {
                SetStatus("请先选择动作资产集合", MessageType.Error);
                return;
            }

            string filePath = EditorUtility.OpenFilePanel(
                "导入动作转移规则表",
                string.Empty,
                "xlsx");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (!string.Equals(Path.GetExtension(filePath), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("文件必须是 xlsx 格式", MessageType.Error);
                return;
            }

            if (!TryReadXlsx(filePath, out List<ActionLinkRow> rows, out string error))
            {
                SetStatus($"导入失败 {error}", MessageType.Error);
                return;
            }

            try
            {
                ApplyRows(rows);
                SetStatus(
                    $"导入完成 共写入 {rows.Count.ToString(CultureInfo.InvariantCulture)} 条规则",
                    MessageType.Info);
            }
            catch (InvalidOperationException exception)
            {
                SetStatus($"导入失败 {exception.Message}", MessageType.Error);
            }
        }

        private void ApplyRows(IReadOnlyList<ActionLinkRow> rows)
        {
            SerializedObject serializedObject = new SerializedObject(_characterActionSetAsset);
            SerializedProperty linksProperty = serializedObject.FindProperty("_links");
            if (linksProperty == null || !linksProperty.isArray)
            {
                throw new InvalidOperationException("未找到动作转移规则列表字段 _links");
            }

            serializedObject.Update();
            Undo.RegisterCompleteObjectUndo(_characterActionSetAsset, "导入动作转移规则表");
            linksProperty.arraySize = rows.Count;

            for (int index = 0; index < rows.Count; index++)
            {
                SerializedProperty linkProperty = linksProperty.GetArrayElementAtIndex(index);
                ActionLinkRow row = rows[index];
                GetRelativeProperty(linkProperty, "_sourceActionId").stringValue = row.SourceActionId;
                GetRelativeProperty(linkProperty, "_targetActionId").stringValue = row.TargetActionId;
                GetRelativeProperty(linkProperty, "_priority").intValue = row.Priority;
                GetRelativeProperty(linkProperty, "_normalizedInterruptionWindowStart").floatValue =
                    row.InterruptionWindowStart;
                GetRelativeProperty(linkProperty, "_normalizedInterruptionWindowEnd").floatValue =
                    row.InterruptionWindowEnd;
                GetRelativeProperty(linkProperty, "_animationBlendSeconds").floatValue =
                    row.AnimationBlendSeconds;

                SerializedProperty intentionProperty = GetRelativeProperty(
                    linkProperty,
                    "_requiredIntention");
                for (int intentionIndex = 0; intentionIndex < IntentionFieldNames.Length; intentionIndex++)
                {
                    GetRelativeProperty(intentionProperty, IntentionFieldNames[intentionIndex]).enumValueIndex =
                        (int)row.Intentions[intentionIndex];
                }

                SerializedProperty factProperty = GetRelativeProperty(linkProperty, "_requiredFact");
                for (int factIndex = 0; factIndex < FactFieldNames.Length; factIndex++)
                {
                    GetRelativeProperty(factProperty, FactFieldNames[factIndex]).enumValueIndex =
                        (int)row.Facts[factIndex];
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(_characterActionSetAsset);
            AssetDatabase.SaveAssetIfDirty(_characterActionSetAsset);
        }

        private static SerializedProperty GetRelativeProperty(
            SerializedProperty parentProperty,
            string propertyName)
        {
            SerializedProperty property = parentProperty.FindPropertyRelative(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"未找到序列化字段 {propertyName}");
            }

            return property;
        }

        private void SetStatus(string message, MessageType messageType)
        {
            _statusMessage = message;
            _statusMessageType = messageType;
            Repaint();
        }

        private static bool TryReadXlsx(
            string filePath,
            out List<ActionLinkRow> rows,
            out string error)
        {
            rows = new List<ActionLinkRow>();
            error = string.Empty;

            try
            {
                using (FileStream fileStream = new FileStream(
                           filePath,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.ReadWrite | FileShare.Delete))
                using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    XDocument workbook = ReadXml(archive, "xl/workbook.xml");
                    string worksheetPath = GetFirstWorksheetPath(archive, workbook);
                    List<string> sharedStrings = ReadSharedStrings(archive);
                    XDocument worksheet = ReadXml(archive, worksheetPath);
                    List<ParsedRow> parsedRows = ReadWorksheetRows(worksheet, sharedStrings);
                    return TryParseRows(parsedRows, out rows, out error);
                }
            }
            catch (InvalidDataException exception)
            {
                error = $"工作簿格式错误 {exception.Message}";
            }
            catch (XmlException exception)
            {
                error = $"工作簿格式错误 {exception.Message}";
            }
            catch (InvalidOperationException exception)
            {
                error = $"工作簿格式错误 {exception.Message}";
            }
            catch (IOException exception)
            {
                error = $"读取文件失败 {exception.Message}";
            }
            catch (UnauthorizedAccessException exception)
            {
                error = $"读取文件失败 {exception.Message}";
            }
            catch (ArgumentException exception)
            {
                error = $"读取文件失败 {exception.Message}";
            }
            catch (NotSupportedException exception)
            {
                error = $"读取文件失败 {exception.Message}";
            }

            return false;
        }

        private static XDocument ReadXml(ZipArchive archive, string entryPath)
        {
            ZipArchiveEntry entry = archive.GetEntry(entryPath);
            if (entry == null)
            {
                throw new InvalidOperationException($"工作簿缺少文件 {entryPath}");
            }

            using (Stream stream = entry.Open())
            {
                return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            }
        }

        private static string GetFirstWorksheetPath(ZipArchive archive, XDocument workbook)
        {
            XElement workbookRoot = workbook.Root;
            XElement sheetsElement = workbookRoot?.Element(SpreadsheetNamespace + "sheets");
            XElement firstSheet = sheetsElement?.Element(SpreadsheetNamespace + "sheet");
            if (firstSheet == null)
            {
                throw new InvalidOperationException("工作簿没有工作表");
            }

            XAttribute relationshipAttribute = firstSheet.Attribute(OfficeRelationshipsNamespace + "id");
            if (relationshipAttribute == null || string.IsNullOrEmpty(relationshipAttribute.Value))
            {
                throw new InvalidOperationException("第一张工作表缺少关系标识");
            }

            XDocument relationships = ReadXml(archive, "xl/_rels/workbook.xml.rels");
            XElement relationship = null;
            XElement relationshipsRoot = relationships.Root;
            if (relationshipsRoot != null)
            {
                foreach (XElement candidate in relationshipsRoot.Elements(
                    PackageRelationshipsNamespace + "Relationship"))
                {
                    if (candidate.Attribute("Id")?.Value == relationshipAttribute.Value)
                    {
                        relationship = candidate;
                        break;
                    }
                }
            }

            string target = relationship?.Attribute("Target")?.Value;
            if (string.IsNullOrEmpty(target))
            {
                throw new InvalidOperationException("第一张工作表缺少文件路径");
            }

            string worksheetPath = ResolveZipPath(target);
            if (!worksheetPath.StartsWith("xl/worksheets/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("第一张工作表文件路径无效");
            }

            return worksheetPath;
        }

        private static string ResolveZipPath(string target)
        {
            string normalizedTarget = target.Replace('\\', '/');
            string path = normalizedTarget.StartsWith("/", StringComparison.Ordinal)
                ? normalizedTarget.TrimStart('/')
                : $"xl/{normalizedTarget}";
            var segments = new List<string>();
            string[] pathParts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < pathParts.Length; index++)
            {
                string pathPart = pathParts[index];
                if (pathPart == ".")
                {
                    continue;
                }

                if (pathPart == "..")
                {
                    if (segments.Count > 0)
                    {
                        segments.RemoveAt(segments.Count - 1);
                    }

                    continue;
                }

                segments.Add(pathPart);
            }

            return string.Join("/", segments);
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            var sharedStrings = new List<string>();
            if (entry == null)
            {
                return sharedStrings;
            }

            XDocument document = ReadXml(archive, "xl/sharedStrings.xml");
            XElement root = document.Root;
            if (root == null)
            {
                throw new InvalidOperationException("共享字符串文件没有根节点");
            }

            foreach (XElement sharedString in root.Elements(SpreadsheetNamespace + "si"))
            {
                sharedStrings.Add(GetTextContent(sharedString));
            }

            return sharedStrings;
        }

        private static List<ParsedRow> ReadWorksheetRows(
            XDocument worksheet,
            IReadOnlyList<string> sharedStrings)
        {
            XElement worksheetRoot = worksheet.Root;
            XElement sheetData = worksheetRoot?.Element(SpreadsheetNamespace + "sheetData");
            if (sheetData == null)
            {
                throw new InvalidOperationException("第一张工作表缺少数据区域");
            }

            var rows = new List<ParsedRow>();
            int fallbackRowNumber = 1;
            foreach (XElement rowElement in sheetData.Elements(SpreadsheetNamespace + "row"))
            {
                int rowNumber = ReadRowNumber(rowElement, fallbackRowNumber);
                fallbackRowNumber = rowNumber + 1;

                var cells = new Dictionary<int, string>();
                int maxColumnIndex = -1;
                foreach (XElement cellElement in rowElement.Elements(SpreadsheetNamespace + "c"))
                {
                    XAttribute referenceAttribute = cellElement.Attribute("r");
                    if (referenceAttribute == null || string.IsNullOrEmpty(referenceAttribute.Value))
                    {
                        throw new InvalidOperationException($"第 {rowNumber} 行存在无列标识单元格");
                    }

                    int columnIndex = GetColumnIndex(referenceAttribute.Value);
                    if (columnIndex < 0)
                    {
                        throw new InvalidOperationException(
                            $"第 {rowNumber} 行单元格列标识无效 {referenceAttribute.Value}");
                    }

                    if (cells.ContainsKey(columnIndex))
                    {
                        throw new InvalidOperationException($"第 {rowNumber} 行存在重复单元格");
                    }

                    cells.Add(columnIndex, ReadCellValue(cellElement, sharedStrings));
                    maxColumnIndex = Math.Max(maxColumnIndex, columnIndex);
                }

                int fieldCount = Math.Max(HeaderColumnCount, maxColumnIndex + 1);
                var fields = new string[fieldCount];
                for (int columnIndex = 0; columnIndex < fieldCount; columnIndex++)
                {
                    fields[columnIndex] = cells.TryGetValue(columnIndex, out string value)
                        ? value
                        : string.Empty;
                }

                rows.Add(new ParsedRow(rowNumber, fields));
            }

            return rows;
        }

        private static int ReadRowNumber(XElement rowElement, int fallbackRowNumber)
        {
            string rowNumberText = rowElement.Attribute("r")?.Value;
            if (string.IsNullOrEmpty(rowNumberText))
            {
                return fallbackRowNumber;
            }

            if (!int.TryParse(rowNumberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int rowNumber)
                || rowNumber < 1)
            {
                throw new InvalidOperationException($"工作表行号无效 {rowNumberText}");
            }

            return rowNumber;
        }

        private static int GetColumnIndex(string cellReference)
        {
            int columnIndex = 0;
            int characterIndex = 0;
            while (characterIndex < cellReference.Length)
            {
                char character = cellReference[characterIndex];
                if (character < 'A' || character > 'Z')
                {
                    break;
                }

                columnIndex = columnIndex * 26 + character - 'A' + 1;
                characterIndex++;
            }

            if (characterIndex == 0 || characterIndex == cellReference.Length)
            {
                return -1;
            }

            while (characterIndex < cellReference.Length)
            {
                char character = cellReference[characterIndex];
                if (character < '0' || character > '9')
                {
                    return -1;
                }

                characterIndex++;
            }

            return columnIndex - 1;
        }

        private static string ReadCellValue(
            XElement cellElement,
            IReadOnlyList<string> sharedStrings)
        {
            string cellType = cellElement.Attribute("t")?.Value;
            if (cellType == "inlineStr")
            {
                return GetTextContent(cellElement.Element(SpreadsheetNamespace + "is"));
            }

            XElement valueElement = cellElement.Element(SpreadsheetNamespace + "v");
            if (valueElement == null)
            {
                return string.Empty;
            }

            string value = valueElement.Value;
            if (cellType != "s")
            {
                return value;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stringIndex)
                || stringIndex < 0
                || stringIndex >= sharedStrings.Count)
            {
                throw new InvalidOperationException($"共享字符串索引无效 {value}");
            }

            return sharedStrings[stringIndex];
        }

        private static string GetTextContent(XElement element)
        {
            if (element == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (XElement textElement in element.Descendants(SpreadsheetNamespace + "t"))
            {
                builder.Append(textElement.Value);
            }

            return builder.ToString();
        }

        private static bool TryParseRows(
            IReadOnlyList<ParsedRow> parsedRows,
            out List<ActionLinkRow> rows,
            out string error)
        {
            rows = new List<ActionLinkRow>();
            error = string.Empty;

            ParsedRow headerRow = null;
            int headerRowIndex = -1;
            for (int index = 0; index < parsedRows.Count; index++)
            {
                if (!IsEmptyRow(parsedRows[index].Fields))
                {
                    headerRow = parsedRows[index];
                    headerRowIndex = index;
                    break;
                }
            }

            if (headerRow == null)
            {
                error = "表格为空";
                return false;
            }

            if (headerRow.Fields.Length != TableHeaders.Length)
            {
                error = $"第 {headerRow.RowNumber} 行表头列数必须是 {TableHeaders.Length}";
                return false;
            }

            for (int columnIndex = 0; columnIndex < TableHeaders.Length; columnIndex++)
            {
                if (headerRow.Fields[columnIndex] != TableHeaders[columnIndex])
                {
                    error =
                        $"第 {headerRow.RowNumber} 行第 {columnIndex + 1} 列表头应为 {TableHeaders[columnIndex]}";
                    return false;
                }
            }

            for (int index = headerRowIndex + 1; index < parsedRows.Count; index++)
            {
                ParsedRow parsedRow = parsedRows[index];
                if (IsEmptyRow(parsedRow.Fields))
                {
                    continue;
                }

                if (parsedRow.Fields.Length != TableHeaders.Length)
                {
                    error = $"第 {parsedRow.RowNumber} 行列数必须是 {TableHeaders.Length}";
                    return false;
                }

                if (!TryParseDataRow(parsedRow.Fields, out ActionLinkRow row, out string rowError))
                {
                    error = $"第 {parsedRow.RowNumber} 行 {rowError}";
                    return false;
                }

                rows.Add(row);
            }

            return true;
        }

        private static bool IsEmptyRow(IReadOnlyList<string> fields)
        {
            for (int index = 0; index < fields.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseDataRow(
            IReadOnlyList<string> fields,
            out ActionLinkRow row,
            out string error)
        {
            row = new ActionLinkRow();
            error = string.Empty;

            if (!TryReadRequiredString(fields[0], TableHeaders[0], out row.SourceActionId, out error)
                || !TryReadRequiredString(fields[1], TableHeaders[1], out row.TargetActionId, out error))
            {
                return false;
            }

            if (!int.TryParse(
                    fields[2],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out row.Priority))
            {
                error = $"{TableHeaders[2]} 必须是整数";
                return false;
            }

            if (!TryParseFloat(fields[3], TableHeaders[3], 0f, 1f, out row.InterruptionWindowStart, out error)
                || !TryParseFloat(fields[4], TableHeaders[4], 0f, 1f, out row.InterruptionWindowEnd, out error)
                || !TryParseFloat(
                    fields[5],
                    TableHeaders[5],
                    0f,
                    float.MaxValue,
                    out row.AnimationBlendSeconds,
                    out error))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(fields[6]) || !string.IsNullOrEmpty(fields[12]))
            {
                error = "第 7 列和第 13 列必须为空";
                return false;
            }

            for (int index = 0; index < row.Intentions.Length; index++)
            {
                if (!TryParseTrilean(
                        fields[IntentionColumnStart + index],
                        TableHeaders[IntentionColumnStart + index],
                        out row.Intentions[index],
                        out error))
                {
                    return false;
                }
            }

            for (int index = 0; index < row.Facts.Length; index++)
            {
                if (!TryParseTrilean(
                        fields[FactColumnStart + index],
                        TableHeaders[FactColumnStart + index],
                        out row.Facts[index],
                        out error))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryReadRequiredString(
            string value,
            string columnName,
            out string result,
            out string error)
        {
            result = value?.Trim();
            if (!string.IsNullOrEmpty(result))
            {
                error = string.Empty;
                return true;
            }

            error = $"{columnName} 不能为空";
            return false;
        }

        private static bool TryParseFloat(
            string value,
            string columnName,
            float minimum,
            float maximum,
            out float result,
            out string error)
        {
            if (!float.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out result)
                || float.IsNaN(result)
                || float.IsInfinity(result)
                || result < minimum
                || result > maximum)
            {
                error = $"{columnName} 必须是 {minimum.ToString(CultureInfo.InvariantCulture)} 到 "
                    + $"{maximum.ToString(CultureInfo.InvariantCulture)} 之间的有限数值";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryParseTrilean(
            string value,
            string columnName,
            out Trilean result,
            out string error)
        {
            switch (value)
            {
                case "F":
                    result = Trilean.False;
                    error = string.Empty;
                    return true;
                case "T":
                    result = Trilean.True;
                    error = string.Empty;
                    return true;
                case "D":
                    result = Trilean.DontCare;
                    error = string.Empty;
                    return true;
                default:
                    result = Trilean.DontCare;
                    error = $"{columnName} 只支持 F T D";
                    return false;
            }
        }

        private sealed class ParsedRow
        {
            public readonly int RowNumber;
            public readonly string[] Fields;

            public ParsedRow(int rowNumber, string[] fields)
            {
                RowNumber = rowNumber;
                Fields = fields;
            }
        }

        private sealed class ActionLinkRow
        {
            public string SourceActionId;
            public string TargetActionId;
            public int Priority;
            public float InterruptionWindowStart;
            public float InterruptionWindowEnd;
            public float AnimationBlendSeconds;
            public readonly Trilean[] Intentions = new Trilean[5];
            public readonly Trilean[] Facts = new Trilean[4];
        }
    }
}
