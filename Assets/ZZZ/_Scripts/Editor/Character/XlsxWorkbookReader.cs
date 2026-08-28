using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    internal sealed class XlsxSheetData
    {
        internal XlsxSheetData(string name, IReadOnlyList<XlsxRowData> rows)
        {
            Name = name;
            Rows = rows;
        }

        internal string Name { get; }
        internal IReadOnlyList<XlsxRowData> Rows { get; }
    }

    internal sealed class XlsxRowData
    {
        internal XlsxRowData(int rowNumber, IReadOnlyList<string> values)
        {
            RowNumber = rowNumber;
            Values = values;
        }

        internal int RowNumber { get; }
        internal IReadOnlyList<string> Values { get; }
    }

    internal static class XlsxWorkbookReader
    {
        private const string WorkbookPath = "xl/workbook.xml";
        private const string WorkbookRelationshipsPath = "xl/_rels/workbook.xml.rels";
        private const string RelationshipIdNamespace =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        internal static IReadOnlyList<XlsxSheetData> Read(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Excel 文件路径不能为空", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("找不到 Excel 文件", filePath);
            }

            using FileStream fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                4096,
                FileOptions.SequentialScan);
            using ZipArchive archive = new ZipArchive(
                fileStream,
                ZipArchiveMode.Read,
                false);

            List<string> sharedStrings = ReadSharedStrings(archive);
            XDocument workbookDocument = LoadRequiredDocument(archive, WorkbookPath);
            XDocument relationshipsDocument =
                LoadRequiredDocument(archive, WorkbookRelationshipsPath);
            Dictionary<string, string> relationships =
                ReadRelationships(relationshipsDocument);

            XElement workbookRoot = workbookDocument.Root;
            if (workbookRoot == null)
            {
                throw new InvalidOperationException("Excel 工作簿 XML 没有根节点");
            }

            XElement sheetsElement = FindRequiredDescendant(workbookRoot, "sheets");
            List<XlsxSheetData> sheets = new List<XlsxSheetData>();

            foreach (XElement sheetElement in ElementsByLocalName(sheetsElement, "sheet"))
            {
                string sheetName = GetRequiredAttribute(sheetElement, "name");
                XAttribute relationshipAttribute =
                    sheetElement.Attribute(XName.Get("id", RelationshipIdNamespace));

                if (relationshipAttribute == null)
                {
                    relationshipAttribute = FindAttributeByLocalName(sheetElement, "id");
                }

                if (relationshipAttribute == null)
                {
                    throw new InvalidOperationException(
                        $"工作表 {sheetName} 没有找到关系 ID");
                }

                string relationshipId = relationshipAttribute.Value;
                if (!relationships.TryGetValue(relationshipId, out string targetPath))
                {
                    throw new InvalidOperationException(
                        $"工作表 {sheetName} 的关系 {relationshipId} 不存在");
                }

                string worksheetPath = ResolveZipPath(WorkbookPath, targetPath);
                XDocument worksheetDocument = LoadRequiredDocument(archive, worksheetPath);
                List<XlsxRowData> rows = ReadRows(worksheetDocument, sharedStrings);
                sheets.Add(new XlsxSheetData(sheetName, rows));
            }

            return sheets;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return new List<string>();
            }

            XDocument document = LoadDocument(entry);
            XElement root = document.Root;
            if (root == null)
            {
                throw new InvalidOperationException("Excel 共享字符串 XML 没有根节点");
            }

            List<string> sharedStrings = new List<string>();
            foreach (XElement sharedStringElement in ElementsByLocalName(root, "si"))
            {
                sharedStrings.Add(ReadRichText(sharedStringElement));
            }

            return sharedStrings;
        }

        private static Dictionary<string, string> ReadRelationships(XDocument document)
        {
            XElement root = document.Root;
            if (root == null)
            {
                throw new InvalidOperationException("Excel 关系 XML 没有根节点");
            }

            Dictionary<string, string> relationships = new Dictionary<string, string>();
            foreach (XElement relationshipElement in ElementsByLocalName(root, "Relationship"))
            {
                string relationshipId = GetRequiredAttribute(relationshipElement, "Id");
                string targetPath = GetRequiredAttribute(relationshipElement, "Target");
                relationships.Add(relationshipId, targetPath);
            }

            return relationships;
        }

        private static List<XlsxRowData> ReadRows(
            XDocument worksheetDocument,
            IReadOnlyList<string> sharedStrings)
        {
            XElement root = worksheetDocument.Root;
            if (root == null)
            {
                throw new InvalidOperationException("Excel 工作表 XML 没有根节点");
            }

            XElement sheetData = FindRequiredDescendant(root, "sheetData");
            List<XlsxRowData> rows = new List<XlsxRowData>();
            int fallbackRowNumber = 1;

            foreach (XElement rowElement in ElementsByLocalName(sheetData, "row"))
            {
                int rowNumber = ReadRowNumber(rowElement, fallbackRowNumber);
                List<string> values = new List<string>();
                int fallbackColumnIndex = 0;

                foreach (XElement cellElement in ElementsByLocalName(rowElement, "c"))
                {
                    string cellReference = GetAttributeValue(cellElement, "r");
                    int columnIndex = ResolveColumnIndex(cellReference, fallbackColumnIndex);
                    EnsureSize(values, columnIndex + 1);
                    values[columnIndex] = ReadCellValue(cellElement, sharedStrings);
                    fallbackColumnIndex = columnIndex + 1;
                }

                rows.Add(new XlsxRowData(rowNumber, values));
                fallbackRowNumber = rowNumber + 1;
            }

            return rows;
        }

        private static int ReadRowNumber(XElement rowElement, int fallbackRowNumber)
        {
            string rowNumberText = GetAttributeValue(rowElement, "r");
            if (int.TryParse(
                    rowNumberText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int rowNumber)
                && rowNumber > 0)
            {
                return rowNumber;
            }

            return fallbackRowNumber;
        }

        private static string ReadCellValue(
            XElement cellElement,
            IReadOnlyList<string> sharedStrings)
        {
            string cellType = GetAttributeValue(cellElement, "t");

            if (cellType == "inlineStr")
            {
                XElement inlineStringElement = FindDescendant(cellElement, "is");
                return inlineStringElement == null
                    ? string.Empty
                    : ReadRichText(inlineStringElement);
            }

            XElement valueElement = FindDescendant(cellElement, "v");
            string rawValue = valueElement == null ? string.Empty : valueElement.Value;

            if (cellType == "s")
            {
                if (!int.TryParse(
                        rawValue,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int sharedStringIndex)
                    || sharedStringIndex < 0
                    || sharedStringIndex >= sharedStrings.Count)
                {
                    throw new InvalidOperationException(
                        $"Excel 单元格的共享字符串索引无效 {rawValue}");
                }

                return sharedStrings[sharedStringIndex];
            }

            if (cellType == "b")
            {
                return rawValue == "1" ? "TRUE" : "FALSE";
            }

            return rawValue;
        }

        private static string ReadRichText(XElement element)
        {
            StringBuilder builder = new StringBuilder();
            foreach (XElement textElement in element.Descendants())
            {
                if (textElement.Name.LocalName == "t")
                {
                    builder.Append(textElement.Value);
                }
            }

            return builder.ToString();
        }

        private static XDocument LoadRequiredDocument(ZipArchive archive, string entryPath)
        {
            ZipArchiveEntry entry = archive.GetEntry(entryPath);
            if (entry == null)
            {
                throw new InvalidOperationException(
                    $"Excel 压缩包缺少文件 {entryPath}");
            }

            return LoadDocument(entry);
        }

        private static XDocument LoadDocument(ZipArchiveEntry entry)
        {
            using Stream stream = entry.Open();
            return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
        }

        private static XElement FindRequiredDescendant(XElement root, string localName)
        {
            XElement element = FindDescendant(root, localName);
            if (element == null)
            {
                throw new InvalidOperationException(
                    $"Excel XML 缺少节点 {localName}");
            }

            return element;
        }

        private static XElement FindDescendant(XElement root, string localName)
        {
            foreach (XElement element in root.Descendants())
            {
                if (element.Name.LocalName == localName)
                {
                    return element;
                }
            }

            return null;
        }

        private static IEnumerable<XElement> ElementsByLocalName(
            XElement parent,
            string localName)
        {
            foreach (XElement element in parent.Elements())
            {
                if (element.Name.LocalName == localName)
                {
                    yield return element;
                }
            }
        }

        private static string GetRequiredAttribute(XElement element, string localName)
        {
            string value = GetAttributeValue(element, localName);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException(
                    $"Excel XML 节点 {element.Name.LocalName} 缺少属性 {localName}");
            }

            return value;
        }

        private static string GetAttributeValue(XElement element, string localName)
        {
            XAttribute attribute = FindAttributeByLocalName(element, localName);
            return attribute == null ? string.Empty : attribute.Value;
        }

        private static XAttribute FindAttributeByLocalName(
            XElement element,
            string localName)
        {
            foreach (XAttribute attribute in element.Attributes())
            {
                if (attribute.Name.LocalName == localName)
                {
                    return attribute;
                }
            }

            return null;
        }

        private static string ResolveZipPath(string basePath, string targetPath)
        {
            string combinedPath = targetPath.StartsWith("/", StringComparison.Ordinal)
                ? targetPath.Substring(1)
                : GetDirectoryPath(basePath) + "/" + targetPath;

            List<string> segments = new List<string>();
            string[] pathParts = combinedPath.Split('/');
            for (int index = 0; index < pathParts.Length; index++)
            {
                string part = pathParts[index];
                if (string.IsNullOrEmpty(part) || part == ".")
                {
                    continue;
                }

                if (part == ".." && segments.Count > 0)
                {
                    segments.RemoveAt(segments.Count - 1);
                    continue;
                }

                segments.Add(part);
            }

            return string.Join("/", segments);
        }

        private static string GetDirectoryPath(string path)
        {
            int separatorIndex = path.LastIndexOf('/');
            return separatorIndex < 0 ? string.Empty : path.Substring(0, separatorIndex);
        }

        private static int ResolveColumnIndex(string cellReference, int fallbackColumnIndex)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return fallbackColumnIndex;
            }

            int columnIndex = 0;
            bool hasLetters = false;
            for (int index = 0; index < cellReference.Length; index++)
            {
                char character = char.ToUpperInvariant(cellReference[index]);
                if (character < 'A' || character > 'Z')
                {
                    break;
                }

                hasLetters = true;
                columnIndex = columnIndex * 26 + character - 'A' + 1;
            }

            return hasLetters ? columnIndex - 1 : fallbackColumnIndex;
        }

        private static void EnsureSize(List<string> values, int requiredSize)
        {
            while (values.Count < requiredSize)
            {
                values.Add(string.Empty);
            }
        }
    }
}
