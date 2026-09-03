using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using GamePlay.Character;

namespace Editor
{
    internal static class CharacterActionCurveBaker
    {
        private const string CumulativeForwardDisplacementPropertyName = "_cumulativeForwardDisplacement";
        private const string RootTranslationZPropertyName = "RootT.z";
        private const string LocalPositionZPropertyName = "m_LocalPosition.z";

        internal static AnimationCurve BakeCumulativeForwardDisplacement(
            AnimationClip sourceClip,
            out string bindingDescription)
        {
            if (sourceClip == null)
            {
                throw new ArgumentNullException(nameof(sourceClip));
            }

            if (sourceClip.length <= 0f)
            {
                throw new InvalidOperationException("源动画时长必须大于 0");
            }

            if (!TryGetRootZCurve(
                    sourceClip,
                    out AnimationCurve sourceCurve,
                    out bindingDescription))
            {
                throw new InvalidOperationException(
                    "源动画没有可用的根节点 Z 位移曲线 支持 RootT 的 z 轴或 Bip001 Root 的本地 z 轴");
            }

            return CreateNormalizedCumulativeCurve(sourceCurve, sourceClip.length);
        }

        internal static bool TryGetRootZCurve(
            AnimationClip sourceClip,
            out AnimationCurve rootZCurve,
            out string bindingDescription)
        {
            rootZCurve = null;
            bindingDescription = string.Empty;

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);
            for (int index = 0; index < bindings.Length; index++)
            {
                EditorCurveBinding binding = bindings[index];
                if (binding.propertyName != RootTranslationZPropertyName)
                {
                    continue;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                if (curve == null || curve.length == 0)
                {
                    continue;
                }

                rootZCurve = curve;
                bindingDescription = "RootT z";
                return true;
            }

            for (int index = 0; index < bindings.Length; index++)
            {
                EditorCurveBinding binding = bindings[index];
                if (binding.propertyName != LocalPositionZPropertyName
                    || !IsSupportedRootNode(binding.path))
                {
                    continue;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                if (curve == null || curve.length == 0)
                {
                    continue;
                }

                rootZCurve = curve;
                bindingDescription = string.IsNullOrEmpty(binding.path)
                    ? "根节点本地 z"
                    : binding.path + " 本地 z";
                return true;
            }

            return false;
        }

        internal static void ApplyToActionAsset(CharacterActionAsset actionAsset, AnimationCurve bakedCurve)
        {
            if (actionAsset == null)
            {
                throw new ArgumentNullException(nameof(actionAsset));
            }

            if (bakedCurve == null)
            {
                throw new ArgumentNullException(nameof(bakedCurve));
            }

            SerializedObject serializedObject = new SerializedObject(actionAsset);
            SerializedProperty curveProperty = serializedObject.FindProperty(
                CumulativeForwardDisplacementPropertyName);
            if (curveProperty == null)
            {
                throw new InvalidOperationException(
                    "未找到动作资产的累计前向位移曲线字段");
            }

            serializedObject.Update();
            Undo.RegisterCompleteObjectUndo(actionAsset, "烘焙动作位移曲线");
            curveProperty.animationCurveValue = bakedCurve;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(actionAsset);
            AssetDatabase.SaveAssetIfDirty(actionAsset);
        }

        private static bool IsSupportedRootNode(string bindingPath)
        {
            int separatorIndex = bindingPath.LastIndexOf('/');
            string nodeName = separatorIndex >= 0
                ? bindingPath.Substring(separatorIndex + 1)
                : bindingPath;

            return nodeName == "Bip001" || nodeName == "Root";
        }

        private static AnimationCurve CreateNormalizedCumulativeCurve(AnimationCurve sourceCurve, float sourceDuration)
        {
            Keyframe[] sourceKeys = sourceCurve.keys;
            float initialValue = sourceCurve.Evaluate(0f);
            List<Keyframe> bakedKeys = new List<Keyframe>(sourceKeys.Length + 2);

            for (int index = 0; index < sourceKeys.Length; index++)
            {
                Keyframe key = sourceKeys[index];
                key.time = Mathf.Clamp01(key.time / sourceDuration);
                key.value -= initialValue;
                key.inTangent = ScaleTangent(key.inTangent, sourceDuration);
                key.outTangent = ScaleTangent(key.outTangent, sourceDuration);
                AddOrReplaceKey(bakedKeys, key);
            }

            if (bakedKeys.Count == 0)
            {
                throw new InvalidOperationException("源动画根节点 Z 位移曲线没有关键帧");
            }

            if (!Mathf.Approximately(bakedKeys[0].time, 0f))
            {
                Keyframe firstKey = bakedKeys[0];
                bakedKeys.Insert(
                    0,
                    new Keyframe(0f, 0f, firstKey.inTangent, firstKey.inTangent));
            }
            else
            {
                Keyframe firstKey = bakedKeys[0];
                firstKey.time = 0f;
                firstKey.value = 0f;
                bakedKeys[0] = firstKey;
            }

            if (!Mathf.Approximately(bakedKeys[bakedKeys.Count - 1].time, 1f))
            {
                Keyframe lastKey = bakedKeys[bakedKeys.Count - 1];
                float finalValue = sourceCurve.Evaluate(sourceDuration) - initialValue;
                bakedKeys.Add(
                    new Keyframe(1f, finalValue, lastKey.outTangent, lastKey.outTangent));
            }

            AnimationCurve bakedCurve = new AnimationCurve(bakedKeys.ToArray())
            {
                preWrapMode = WrapMode.ClampForever,
                postWrapMode = WrapMode.ClampForever
            };
            return bakedCurve;
        }

        private static float ScaleTangent(float tangent, float sourceDuration)
        {
            return float.IsInfinity(tangent) ? tangent : tangent * sourceDuration;
        }

        private static void AddOrReplaceKey(List<Keyframe> keys, Keyframe key)
        {
            if (keys.Count > 0 && Mathf.Approximately(keys[keys.Count - 1].time, key.time))
            {
                keys[keys.Count - 1] = key;
                return;
            }

            keys.Add(key);
        }
    }

    internal sealed class CharacterActionCurveBakeWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ZZZ/Character/烘焙动作位移曲线";

        [SerializeField]
        private AnimationClip _sourceClip;
        [SerializeField]
        private CharacterActionAsset _targetActionAsset;

        private string _statusMessage = string.Empty;
        private MessageType _statusMessageType = MessageType.Info;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            CharacterActionCurveBakeWindow window =
                GetWindow<CharacterActionCurveBakeWindow>();
            window.titleContent = new GUIContent("动作位移曲线烘焙");
            window.minSize = new Vector2(520f, 260f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("动作位移曲线烘焙", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            _sourceClip = (AnimationClip)EditorGUILayout.ObjectField(
                new GUIContent("源 AnimationClip", "可选择 FBX 中的 AnimationClip 子资产"),
                _sourceClip,
                typeof(AnimationClip),
                false);

            _targetActionAsset = (CharacterActionAsset)EditorGUILayout.ObjectField(
                new GUIContent("目标 CharacterActionAsset", "将写入累计前向位移曲线"),
                _targetActionAsset,
                typeof(CharacterActionAsset),
                false);

            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(
                "读取源动画的根节点 Z 位移 写入目标动作资产的累计前向位移曲线 时间会归一化到 0 到 1 烘焙会覆盖目标曲线 操作支持 Undo",
                MessageType.Info);

            string validationMessage = GetValidationMessage();
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(validationMessage)))
            {
                if (GUILayout.Button("烘焙并保存动作位移曲线", GUILayout.Height(30f)))
                {
                    BakeCurve();
                }
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox(_statusMessage, _statusMessageType);
            }
        }

        private string GetValidationMessage()
        {
            if (_sourceClip == null)
            {
                return "请指定源 AnimationClip";
            }

            if (_targetActionAsset == null)
            {
                return "请指定目标 CharacterActionAsset";
            }

            if (_sourceClip.length <= 0f)
            {
                return "源动画时长必须大于 0";
            }

            if (!CharacterActionCurveBaker.TryGetRootZCurve(
                    _sourceClip,
                    out _,
                    out _))
            {
                return "源动画没有可用的根节点 Z 位移曲线 支持 RootT 的 z 轴或 Bip001 Root 的本地 z 轴";
            }

            return string.Empty;
        }

        private void BakeCurve()
        {
            AnimationCurve bakedCurve = CharacterActionCurveBaker.BakeCumulativeForwardDisplacement(
                _sourceClip,
                out string bindingDescription);
            CharacterActionCurveBaker.ApplyToActionAsset(_targetActionAsset, bakedCurve);

            float finalDisplacement = bakedCurve.Evaluate(1f);
            SetStatus(
                $"烘焙完成 使用 {bindingDescription} 共写入 {bakedCurve.length} 个关键帧 末端累计位移 {finalDisplacement}",
                MessageType.Info);
            ShowNotification(new GUIContent("烘焙完成"));
        }

        private void SetStatus(string message, MessageType messageType)
        {
            _statusMessage = message;
            _statusMessageType = messageType;
            Repaint();
        }
    }
}
