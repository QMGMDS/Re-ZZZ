using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using GamePlay.Data;

namespace GamePlay.Editor
{
    [CustomEditor(typeof(CharacterActionSetAsset))]
    internal sealed class CharacterActionSetAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _actionsProperty;
        private SerializedProperty _linksProperty;

        private void OnEnable()
        {
            _actionsProperty = serializedObject.FindProperty("_actions");
            _linksProperty = serializedObject.FindProperty("_links");

            if (_actionsProperty == null || _linksProperty == null)
            {
                throw new InvalidOperationException(
                    "CharacterActionSetAsset 的序列化字段不完整");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                _actionsProperty,
                new GUIContent("动作列表"),
                true);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("有向边配置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "表头：出边 去边 角色意图 角色事实 优先级 动画过渡" +
                "条件示例：F_Attack+T_Move 和 D_Death+D_LogicalProgress" +
                "表格每一行导入为一条有向边 导入成功后会全量替换当前有向边配置",
                MessageType.Info);

            if (GUILayout.Button("从 XLSX 导入有向边", GUILayout.Height(30f)))
            {
                serializedObject.ApplyModifiedProperties();
                ImportLinks();
                serializedObject.Update();
            }

            EditorGUILayout.PropertyField(
                _linksProperty,
                new GUIContent("已导入有向边"),
                true);

            serializedObject.ApplyModifiedProperties();
        }

        private void ImportLinks()
        {
            string xlsxPath = EditorUtility.OpenFilePanel(
                "选择角色动作有向边表",
                Application.dataPath,
                "xlsx");

            if (string.IsNullOrEmpty(xlsxPath))
            {
                return;
            }

            CharacterActionSetAsset actionSet = (CharacterActionSetAsset)target;

            try
            {
                IReadOnlyList<CharacterActionLinkImportRow> importedRows =
                    CharacterActionLinkXlsxImporter.ReadAndValidate(xlsxPath, actionSet);

                WriteLinks(actionSet, importedRows);
                EditorUtility.SetDirty(actionSet);
                AssetDatabase.SaveAssetIfDirty(actionSet);
                EditorUtility.DisplayDialog(
                    "导入完成",
                    $"有向边数量 {importedRows.Count}",
                    "确定");
                Repaint();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, actionSet);
                EditorUtility.DisplayDialog(
                    "导入失败",
                    exception.Message,
                    "确定");
            }
        }

        private static void WriteLinks(
            CharacterActionSetAsset actionSet,
            IReadOnlyList<CharacterActionLinkImportRow> importedRows)
        {
            Undo.RegisterCompleteObjectUndo(actionSet, "导入角色动作有向边");

            SerializedObject serializedActionSet = new SerializedObject(actionSet);
            serializedActionSet.Update();
            SerializedProperty linksProperty = serializedActionSet.FindProperty("_links");

            if (linksProperty == null)
            {
                throw new InvalidOperationException(
                    "CharacterActionSetAsset 没有找到 _links 序列化字段");
            }

            linksProperty.arraySize = importedRows.Count;

            for (int index = 0; index < importedRows.Count; index++)
            {
                SerializedProperty linkProperty = linksProperty.GetArrayElementAtIndex(index);
                WriteLink(linkProperty, importedRows[index]);
            }

            serializedActionSet.ApplyModifiedProperties();
        }

        private static void WriteLink(
            SerializedProperty linkProperty,
            CharacterActionLinkImportRow importedRow)
        {
            SerializedProperty fromActionIdProperty =
                linkProperty.FindPropertyRelative("_fromActionId");
            SerializedProperty toActionIdProperty =
                linkProperty.FindPropertyRelative("_toActionId");
            SerializedProperty priorityProperty =
                linkProperty.FindPropertyRelative("_priority");
            SerializedProperty intentionProperty =
                linkProperty.FindPropertyRelative("_requiredIntention");
            SerializedProperty factProperty =
                linkProperty.FindPropertyRelative("_requiredFact");
            SerializedProperty transitionProperty = linkProperty.FindPropertyRelative(
                "_animationTransitionDurationSeconds");
            SerializedProperty attackProperty = intentionProperty.FindPropertyRelative("_attack");
            SerializedProperty moveProperty = intentionProperty.FindPropertyRelative("_move");
            SerializedProperty deathProperty = factProperty.FindPropertyRelative("_death");
            SerializedProperty logicalProgressProperty = factProperty.FindPropertyRelative(
                "_logicalProgress");

            if (fromActionIdProperty == null ||
                toActionIdProperty == null ||
                priorityProperty == null ||
                intentionProperty == null ||
                factProperty == null ||
                transitionProperty == null ||
                attackProperty == null ||
                moveProperty == null ||
                deathProperty == null ||
                logicalProgressProperty == null)
            {
                throw new InvalidOperationException(
                    "CharacterActionLink 的序列化字段不完整");
            }

            fromActionIdProperty.stringValue = importedRow.FromActionId;
            toActionIdProperty.stringValue = importedRow.ToActionId;
            priorityProperty.intValue = importedRow.Priority;
            attackProperty.intValue = (int)importedRow.RequiredIntention.Attack;
            moveProperty.intValue = (int)importedRow.RequiredIntention.Move;
            deathProperty.intValue = (int)importedRow.RequiredFact.Death;
            logicalProgressProperty.intValue =
                (int)importedRow.RequiredFact.LogicalProgress;
            transitionProperty.floatValue = importedRow.AnimationTransitionDurationSeconds;
        }
    }
}
