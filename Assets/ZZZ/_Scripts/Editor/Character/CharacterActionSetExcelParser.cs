using System;
using System.Collections.Generic;
using System.Globalization;

using GamePlay.Character;

namespace Editor
{
    internal sealed class ParsedTransitionLink
    {
        internal ParsedTransitionLink(
            int rowNumber,
            string fromActionId,
            string toActionId,
            float interruptWindowStartProgress,
            float interruptWindowEndProgress,
            int priority,
            float animationTransitionDurationSeconds,
            Trilean move,
            Trilean attack,
            Trilean evade,
            Trilean skill,
            Trilean ultimate,
            Trilean death,
            Trilean hit,
            Trilean switchIn,
            Trilean switchOut)
        {
            RowNumber = rowNumber;
            FromActionId = fromActionId;
            ToActionId = toActionId;
            InterruptWindowStartProgress = interruptWindowStartProgress;
            InterruptWindowEndProgress = interruptWindowEndProgress;
            Priority = priority;
            AnimationTransitionDurationSeconds = animationTransitionDurationSeconds;
            Move = move;
            Attack = attack;
            Evade = evade;
            Skill = skill;
            Ultimate = ultimate;
            Death = death;
            Hit = hit;
            SwitchIn = switchIn;
            SwitchOut = switchOut;
        }

        internal int RowNumber { get; }
        internal string FromActionId { get; }
        internal string ToActionId { get; }
        internal float InterruptWindowStartProgress { get; }
        internal float InterruptWindowEndProgress { get; }
        internal int Priority { get; }
        internal Trilean Move { get; }
        internal Trilean Attack { get; }
        internal Trilean Evade { get; }
        internal Trilean Skill { get; }
        internal Trilean Ultimate { get; }
        internal Trilean Death { get; }
        internal Trilean Hit { get; }
        internal Trilean SwitchIn { get; }
        internal Trilean SwitchOut { get; }
        internal float AnimationTransitionDurationSeconds { get; }
    }

    internal static class CharacterActionSetExcelParser
    {
        private const string InterruptWindowEndHeader = "打断窗口终点";

        private static readonly string[] RequiredHeaders =
        {
            "出边",
            "去边",
            "打断窗口起点",
            InterruptWindowEndHeader,
            "优先级",
            "动画过渡",
            "Move",
            "Evade",
            "Attack",
            "Skill",
            "Ultimate",
            "Death",
            "Hit",
            "SwitchIn",
            "SwitchOut"
        };

        private static readonly HashSet<string> RequiredHeaderSet =
            new HashSet<string>(RequiredHeaders, StringComparer.Ordinal);

        internal static IReadOnlyList<ParsedTransitionLink> Parse(XlsxSheetData sheet)
        {
            if (sheet == null)
            {
                throw new ArgumentNullException(nameof(sheet));
            }

            Dictionary<string, int> headerMap = null;
            int headerRowIndex = -1;
            string headerError = string.Empty;

            for (int rowIndex = 0; rowIndex < sheet.Rows.Count; rowIndex++)
            {
                Dictionary<string, int> candidate;
                string candidateError;
                bool looksLikeHeader;
                HeaderCheckResult result = TryBuildHeaderMap(
                    sheet.Rows[rowIndex],
                    out candidate,
                    out looksLikeHeader,
                    out candidateError);

                if (result == HeaderCheckResult.Valid)
                {
                    headerMap = candidate;
                    headerRowIndex = rowIndex;
                    break;
                }

                if (looksLikeHeader && string.IsNullOrEmpty(headerError))
                {
                    headerError = candidateError;
                }
            }

            if (headerRowIndex < 0)
            {
                throw new InvalidOperationException(
                    string.IsNullOrEmpty(headerError)
                        ? "没有找到完整表头 必须包含出边 去边 打断窗口起点 打断窗口终点 优先级 动画过渡 Move Evade Attack Skill Ultimate Death Hit SwitchIn SwitchOut"
                        : headerError);
            }

            List<ParsedTransitionLink> links = new List<ParsedTransitionLink>();
            List<string> errors = new List<string>();

            for (int rowIndex = headerRowIndex + 1; rowIndex < sheet.Rows.Count; rowIndex++)
            {
                XlsxRowData row = sheet.Rows[rowIndex];
                if (IsBlankDataRow(row, headerMap))
                {
                    continue;
                }

                try
                {
                    links.Add(ParseRow(row, headerMap));
                }
                catch (InvalidOperationException exception)
                {
                    errors.Add(exception.Message);
                }
            }

            ValidateDuplicateLinks(links, errors);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join("\n", errors));
            }

            return links;
        }

        private static HeaderCheckResult TryBuildHeaderMap(
            XlsxRowData row,
            out Dictionary<string, int> headerMap,
            out bool looksLikeHeader,
            out string error)
        {
            headerMap = new Dictionary<string, int>(StringComparer.Ordinal);
            looksLikeHeader = false;
            error = string.Empty;

            List<string> unknownHeaders = new List<string>();
            List<string> duplicateHeaders = new List<string>();

            for (int column = 0; column < row.Values.Count; column++)
            {
                string header = NormalizeHeader(row.Values[column]);
                if (string.IsNullOrEmpty(header))
                {
                    continue;
                }

                if (!RequiredHeaderSet.Contains(header))
                {
                    unknownHeaders.Add(header);
                    continue;
                }

                looksLikeHeader = true;
                if (headerMap.ContainsKey(header))
                {
                    if (!duplicateHeaders.Contains(header))
                    {
                        duplicateHeaders.Add(header);
                    }

                    continue;
                }

                headerMap.Add(header, column);
            }

            if (!looksLikeHeader)
            {
                return HeaderCheckResult.NotHeader;
            }

            if (duplicateHeaders.Count > 0)
            {
                error =
                    $"第 {row.RowNumber} 行的表头重复 {string.Join("、", duplicateHeaders)}";
                return HeaderCheckResult.Invalid;
            }

            if (unknownHeaders.Count > 0)
            {
                error =
                    $"第 {row.RowNumber} 行存在新表未定义的表头 {string.Join("、", unknownHeaders)}";
                return HeaderCheckResult.Invalid;
            }

            List<string> missingHeaders = new List<string>();
            for (int index = 0; index < RequiredHeaders.Length; index++)
            {
                string requiredHeader = RequiredHeaders[index];
                if (!headerMap.ContainsKey(requiredHeader))
                {
                    missingHeaders.Add(requiredHeader);
                }
            }

            if (missingHeaders.Count > 0)
            {
                error =
                    $"第 {row.RowNumber} 行缺少新表必需表头 {string.Join("、", missingHeaders)}";
                return HeaderCheckResult.Invalid;
            }

            return HeaderCheckResult.Valid;
        }

        private static ParsedTransitionLink ParseRow(
            XlsxRowData row,
            IReadOnlyDictionary<string, int> headerMap)
        {
            string fromActionId = GetCell(row, headerMap, "出边").Trim();
            string toActionId = GetCell(row, headerMap, "去边").Trim();
            if (string.IsNullOrEmpty(fromActionId) || string.IsNullOrEmpty(toActionId))
            {
                throw new InvalidOperationException(
                    $"第 {row.RowNumber} 行的出边和去边必须同时填写");
            }

            float interruptWindowStartProgress = ParseFloat(
                GetCell(row, headerMap, "打断窗口起点"),
                row.RowNumber,
                "打断窗口起点");
            ValidateProgress(
                interruptWindowStartProgress,
                row.RowNumber,
                "打断窗口起点");

            float interruptWindowEndProgress = ParseFloat(
                GetCell(row, headerMap, InterruptWindowEndHeader),
                row.RowNumber,
                InterruptWindowEndHeader);
            ValidateProgress(
                interruptWindowEndProgress,
                row.RowNumber,
                InterruptWindowEndHeader);

            if (interruptWindowStartProgress > interruptWindowEndProgress)
            {
                throw new InvalidOperationException(
                    $"第 {row.RowNumber} 行的打断窗口起点不能晚于终点");
            }

            int priority = ParseInt(
                GetCell(row, headerMap, "优先级"),
                row.RowNumber,
                "优先级");
            float transitionDurationSeconds = ParseFloat(
                GetCell(row, headerMap, "动画过渡"),
                row.RowNumber,
                "动画过渡");
            if (transitionDurationSeconds < 0f)
            {
                throw new InvalidOperationException(
                    $"第 {row.RowNumber} 行的动画过渡秒数不能小于 0");
            }

            return new ParsedTransitionLink(
                row.RowNumber,
                fromActionId,
                toActionId,
                interruptWindowStartProgress,
                interruptWindowEndProgress,
                priority,
                transitionDurationSeconds,
                ParseTrilean(GetCell(row, headerMap, "Move"), row.RowNumber, "Move"),
                ParseTrilean(GetCell(row, headerMap, "Attack"), row.RowNumber, "Attack"),
                ParseTrilean(GetCell(row, headerMap, "Evade"), row.RowNumber, "Evade"),
                ParseTrilean(GetCell(row, headerMap, "Skill"), row.RowNumber, "Skill"),
                ParseTrilean(
                    GetCell(row, headerMap, "Ultimate"),
                    row.RowNumber,
                    "Ultimate"),
                ParseTrilean(GetCell(row, headerMap, "Death"), row.RowNumber, "Death"),
                ParseTrilean(GetCell(row, headerMap, "Hit"), row.RowNumber, "Hit"),
                ParseTrilean(
                    GetCell(row, headerMap, "SwitchIn"),
                    row.RowNumber,
                    "SwitchIn"),
                ParseTrilean(
                    GetCell(row, headerMap, "SwitchOut"),
                    row.RowNumber,
                    "SwitchOut"));
        }

        private static void ValidateProgress(float value, int rowNumber, string header)
        {
            if (value < 0f || value > 1f)
            {
                throw new InvalidOperationException(
                    $"第 {rowNumber} 行的 {header} 必须位于 0 到 1 之间");
            }
        }

        private static void ValidateDuplicateLinks(
            IReadOnlyList<ParsedTransitionLink> links,
            List<string> errors)
        {
            Dictionary<string, int> firstRowByLink =
                new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < links.Count; index++)
            {
                ParsedTransitionLink link = links[index];
                string key = link.FromActionId + "\u001F" + link.ToActionId;
                if (firstRowByLink.TryGetValue(key, out int firstRowNumber))
                {
                    errors.Add(
                        $"第 {link.RowNumber} 行的动作链接 {link.FromActionId} -> {link.ToActionId} 重复 已在第 {firstRowNumber} 行配置");
                    continue;
                }

                firstRowByLink.Add(key, link.RowNumber);
            }
        }

        private static bool IsBlankDataRow(
            XlsxRowData row,
            IReadOnlyDictionary<string, int> headerMap)
        {
            for (int index = 0; index < RequiredHeaders.Length; index++)
            {
                if (!string.IsNullOrWhiteSpace(
                        GetCell(row, headerMap, RequiredHeaders[index])))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetCell(
            XlsxRowData row,
            IReadOnlyDictionary<string, int> headerMap,
            string header)
        {
            int columnIndex = headerMap[header];
            return columnIndex < row.Values.Count
                ? row.Values[columnIndex]
                : string.Empty;
        }

        private static float ParseFloat(string value, int rowNumber, string header)
        {
            if (float.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float result)
                || float.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out result))
            {
                if (!float.IsNaN(result) && !float.IsInfinity(result))
                {
                    return result;
                }
            }

            throw new InvalidOperationException(
                $"第 {rowNumber} 行的 {header} 不是合法数字 当前值 {value}");
        }

        private static int ParseInt(string value, int rowNumber, string header)
        {
            if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int result)
                || int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.CurrentCulture,
                    out result))
            {
                return result;
            }

            throw new InvalidOperationException(
                $"第 {rowNumber} 行的 {header} 不是合法整数 当前值 {value}");
        }

        private static Trilean ParseTrilean(string value, int rowNumber, string header)
        {
            string normalized = value.Trim();
            switch (normalized)
            {
                case "T":
                    return Trilean.True;
                case "F":
                    return Trilean.False;
                case "D":
                    return Trilean.DontCare;
                default:
                    throw new InvalidOperationException(
                        $"第 {rowNumber} 行的 {header} 必须填写 T F 或 D 当前值 {value}");
            }
        }

        private static string NormalizeHeader(string value)
        {
            return value.Replace("\uFEFF", string.Empty).Trim();
        }

        private enum HeaderCheckResult
        {
            NotHeader,
            Invalid,
            Valid
        }
    }
}
