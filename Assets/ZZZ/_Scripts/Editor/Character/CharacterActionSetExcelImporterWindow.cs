using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using GamePlay.Character;

namespace Editor
{
    internal sealed class CharacterActionSetExcelImporterWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ZZZ/Character/导入动作转向边";
        private const int MaxValidationMessages = 12;

        [SerializeField] private CharacterActionSetAsset _targetActionSet;
        [SerializeField] private string _excelFilePath = string.Empty;
        [SerializeField] private int _sheetIndex;

        [NonSerialized] private List<XlsxSheetData> _sheets = new List<XlsxSheetData>();
        [NonSerialized]
        private List<ParsedTransitionLink> _parsedLinks =
            new List<ParsedTransitionLink>();
        [NonSerialized] private string _readError = string.Empty;
        [NonSerialized] private string _loadedFilePath = string.Empty;

        [MenuItem(MenuPath)]
        private static void OpenFromMenu()
        {
            Open(null);
        }

        internal static void Open(CharacterActionSetAsset targetActionSet)
        {
            CharacterActionSetExcelImporterWindow window =
                GetWindow<CharacterActionSetExcelImporterWindow>();

            window.titleContent = new GUIContent("导入动作转向边");
            window.minSize = new Vector2(620f, 420f);
            window._targetActionSet = targetActionSet;
            window.Show();
        }

        private void OnEnable()
        {
            if (_sheets == null)
            {
                _sheets = new List<XlsxSheetData>();
            }

            if (_parsedLinks == null)
            {
                _parsedLinks = new List<ParsedTransitionLink>();
            }

            if (!string.IsNullOrWhiteSpace(_excelFilePath))
            {
                LoadWorkbook();
            }
        }

        private void OnGUI()
        {
            DrawTargetSection();
            DrawExcelSection();
            DrawSheetSection();
            DrawReadResult();
            DrawImportButton();
        }

        private void DrawTargetSection()
        {
            EditorGUILayout.LabelField("导入目标", EditorStyles.boldLabel);

            _targetActionSet = (CharacterActionSetAsset)EditorGUILayout.ObjectField(
                new GUIContent("动作资产集合", "只会替换目标资产中的转向边规则"),
                _targetActionSet,
                typeof(CharacterActionSetAsset),
                false);

            EditorGUILayout.Space(8f);
        }

        private void DrawExcelSection()
        {
            EditorGUILayout.LabelField("Excel 文件", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            string nextPath = EditorGUILayout.TextField(
                new GUIContent("文件路径"),
                _excelFilePath);

            if (!string.Equals(nextPath, _excelFilePath, StringComparison.Ordinal))
            {
                _excelFilePath = nextPath;
                ClearLoadedWorkbook();
            }

            if (GUILayout.Button("选择", GUILayout.Width(70f)))
            {
                string selectedPath = EditorUtility.OpenFilePanel(
                    "选择动作转向边 Excel",
                    Application.dataPath,
                    "xlsx");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _excelFilePath = selectedPath;
                    LoadWorkbook();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("读取 Excel", GUILayout.Height(24f)))
            {
                LoadWorkbook();
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawSheetSection()
        {
            if (_sheets.Count == 0)
            {
                return;
            }

            string[] sheetNames = new string[_sheets.Count];
            for (int index = 0; index < _sheets.Count; index++)
            {
                sheetNames[index] = _sheets[index].Name;
            }

            _sheetIndex = Mathf.Clamp(_sheetIndex, 0, _sheets.Count - 1);
            int nextSheetIndex = EditorGUILayout.Popup(
                new GUIContent("工作表"),
                _sheetIndex,
                sheetNames);

            if (nextSheetIndex != _sheetIndex)
            {
                _sheetIndex = nextSheetIndex;
                ParseSelectedSheet();
            }
        }

        private void DrawReadResult()
        {
            if (!string.IsNullOrEmpty(_readError))
            {
                EditorGUILayout.HelpBox(_readError, MessageType.Error);
                return;
            }

            if (_sheets.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "请选择 Excel 文件并读取",
                    MessageType.Info);
                return;
            }

            string validationMessage = GetValidationMessage();
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Error);
                return;
            }

            EditorGUILayout.HelpBox(
                $"工作表 {_sheets[_sheetIndex].Name} 已读取 {_parsedLinks.Count} 条转向边规则",
                MessageType.Info);
        }

        private void DrawImportButton()
        {
            string validationMessage = GetValidationMessage();
            bool canImport = string.IsNullOrEmpty(validationMessage);

            using (new EditorGUI.DisabledScope(!canImport))
            {
                if (GUILayout.Button("导入并替换转向边规则", GUILayout.Height(32f)))
                {
                    ImportLinks();
                }
            }
        }

        private void LoadWorkbook()
        {
            ClearLoadedWorkbook();

            if (string.IsNullOrWhiteSpace(_excelFilePath))
            {
                return;
            }

            try
            {
                _sheets = new List<XlsxSheetData>(XlsxWorkbookReader.Read(_excelFilePath));
                if (_sheets.Count == 0)
                {
                    throw new InvalidOperationException("Excel 文件没有工作表");
                }

                _loadedFilePath = _excelFilePath;
                _sheetIndex = Mathf.Clamp(_sheetIndex, 0, _sheets.Count - 1);
                ParseSelectedSheet();
            }
            catch (Exception exception)
            {
                _readError = exception.Message;
            }

            Repaint();
        }

        private void ClearLoadedWorkbook()
        {
            _sheets.Clear();
            _parsedLinks.Clear();
            _readError = string.Empty;
            _loadedFilePath = string.Empty;
        }

        private void ParseSelectedSheet()
        {
            _parsedLinks.Clear();
            _readError = string.Empty;

            if (_sheets.Count == 0)
            {
                return;
            }

            try
            {
                XlsxSheetData sheet = _sheets[_sheetIndex];
                _parsedLinks.AddRange(ParseSheet(sheet));
            }
            catch (Exception exception)
            {
                _readError = exception.Message;
            }

            Repaint();
        }

        private static List<ParsedTransitionLink> ParseSheet(XlsxSheetData sheet)
        {
            return new List<ParsedTransitionLink>(
                CharacterActionSetExcelParser.Parse(sheet));
        }

        private string GetValidationMessage()
        {
            if (_targetActionSet == null)
            {
                return "请指定动作资产集合";
            }

            if (string.IsNullOrWhiteSpace(_excelFilePath))
            {
                return "请指定 Excel 文件";
            }

            if (!string.IsNullOrEmpty(_readError))
            {
                return _readError;
            }

            if (!string.Equals(_loadedFilePath, _excelFilePath, StringComparison.Ordinal))
            {
                return "请先读取 Excel 文件";
            }

            if (_sheets.Count == 0)
            {
                return "Excel 文件没有工作表";
            }

            if (_parsedLinks.Count == 0)
            {
                return "当前工作表没有转向边规则";
            }

            List<string> errors = ValidateTargetActionIds();
            return errors.Count == 0 ? string.Empty : string.Join("\n", errors);
        }

        private List<string> ValidateTargetActionIds()
        {
            SerializedObject serializedTarget = new SerializedObject(_targetActionSet);
            SerializedProperty actionsProperty = serializedTarget.FindProperty("_actions");
            if (actionsProperty == null)
            {
                throw new InvalidOperationException(
                    "动作资产集合没有找到 _actions 序列化字段");
            }

            HashSet<string> actionIds = new HashSet<string>();
            List<string> errors = new List<string>();

            for (int index = 0; index < actionsProperty.arraySize; index++)
            {
                CharacterActionAsset action =
                    actionsProperty.GetArrayElementAtIndex(index).objectReferenceValue
                    as CharacterActionAsset;

                if (action == null)
                {
                    AddValidationMessage(
                        errors,
                        $"动作列表第 {index + 1} 项为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(action.Id))
                {
                    AddValidationMessage(
                        errors,
                        $"动作列表第 {index + 1} 项的动作 ID 为空");
                    continue;
                }

                if (!actionIds.Add(action.Id))
                {
                    AddValidationMessage(
                        errors,
                        $"动作 ID 重复 {action.Id}");
                }
            }

            for (int index = 0; index < _parsedLinks.Count; index++)
            {
                ParsedTransitionLink link = _parsedLinks[index];
                if (!actionIds.Contains(link.FromActionId))
                {
                    AddValidationMessage(
                        errors,
                        $"第 {link.RowNumber} 行的出边动作不存在 {link.FromActionId}");
                }

                if (!actionIds.Contains(link.ToActionId))
                {
                    AddValidationMessage(
                        errors,
                        $"第 {link.RowNumber} 行的去边动作不存在 {link.ToActionId}");
                }
            }

            return errors;
        }

        private static void AddValidationMessage(List<string> messages, string message)
        {
            if (messages.Count < MaxValidationMessages)
            {
                messages.Add(message);
            }
        }

        private void ImportLinks()
        {
            string validationMessage = GetValidationMessage();
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorUtility.DisplayDialog("导入失败", validationMessage, "确定");
                return;
            }

            try
            {
                SerializedObject serializedTarget = new SerializedObject(_targetActionSet);
                SerializedProperty linksProperty = serializedTarget.FindProperty("_links");
                if (linksProperty == null)
                {
                    throw new InvalidOperationException(
                        "动作资产集合没有找到 _links 序列化字段");
                }

                Undo.RegisterCompleteObjectUndo(_targetActionSet, "导入角色动作转向边");
                serializedTarget.Update();
                linksProperty.arraySize = _parsedLinks.Count;

                for (int index = 0; index < _parsedLinks.Count; index++)
                {
                    WriteLink(
                        linksProperty.GetArrayElementAtIndex(index),
                        _parsedLinks[index]);
                }

                serializedTarget.ApplyModifiedProperties();
                EditorUtility.SetDirty(_targetActionSet);
                AssetDatabase.SaveAssetIfDirty(_targetActionSet);
                Selection.activeObject = _targetActionSet;
                EditorGUIUtility.PingObject(_targetActionSet);

                ShowNotification(
                    new GUIContent($"导入完成 转向边 {_parsedLinks.Count} 条"));
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog("导入失败", exception.Message, "确定");
            }
        }

        private static void WriteLink(
            SerializedProperty linkProperty,
            ParsedTransitionLink link)
        {
            GetRequiredRelative(linkProperty, "_fromActionId").stringValue = link.FromActionId;
            GetRequiredRelative(linkProperty, "_toActionId").stringValue = link.ToActionId;
            GetRequiredRelative(linkProperty, "_interruptProgress").floatValue =
                link.InterruptWindowStartProgress;
            GetRequiredRelative(linkProperty, "_interruptEndProgress").floatValue =
                link.InterruptWindowEndProgress;
            GetRequiredRelative(linkProperty, "_priority").intValue = link.Priority;
            GetRequiredRelative(linkProperty, "_animationTransitionDurationSeconds").floatValue =
                link.AnimationTransitionDurationSeconds;

            SerializedProperty requiredIntention =
                GetRequiredRelative(linkProperty, "_requiredIntention");
            SetTrilean(requiredIntention, "_move", link.Move);
            SetTrilean(requiredIntention, "_attack", link.Attack);
            SetTrilean(requiredIntention, "_evade", link.Evade);
            SetTrilean(requiredIntention, "_skill", link.Skill);
            SetTrilean(requiredIntention, "_ultimate", link.Ultimate);

            SerializedProperty requiredFact = GetRequiredRelative(linkProperty, "_requiredFact");
            SetTrilean(requiredFact, "_death", link.Death);
            SetTrilean(requiredFact, "_hit", link.Hit);
            SetTrilean(requiredFact, "_switchIn", link.SwitchIn);
            SetTrilean(requiredFact, "_switchOut", link.SwitchOut);
        }

        private static void SetTrilean(
            SerializedProperty parentProperty,
            string propertyName,
            Trilean value)
        {
            SerializedProperty property = GetRequiredRelative(parentProperty, propertyName);
            string enumName = value.ToString();
            for (int index = 0; index < property.enumNames.Length; index++)
            {
                if (property.enumNames[index] == enumName)
                {
                    property.enumValueIndex = index;
                    return;
                }
            }

            throw new InvalidOperationException(
                $"序列化字段 {propertyName} 没有找到枚举值 {enumName}");
        }

        private static SerializedProperty GetRequiredRelative(
            SerializedProperty parentProperty,
            string propertyName)
        {
            SerializedProperty property = parentProperty.FindPropertyRelative(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"转向边没有找到序列化字段 {propertyName}");
            }

            return property;
        }

    }
}
