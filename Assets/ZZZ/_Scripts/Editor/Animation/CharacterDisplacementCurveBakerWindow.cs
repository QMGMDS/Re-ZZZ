using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using GamePlay.Character;

namespace Editor
{
    internal static class CharacterDisplacementCurveBaker
    {
        private const string RootTzPropertyName = "RootT.z";
        private const string LocalPositionZPropertyName = "m_LocalPosition.z";

        internal static AnimationCurve BuildCurve(
            AnimationClip sourceClip,
            float targetDurationSeconds,
            int sampleRate,
            bool invertDisplacement,
            bool linearTangents,
            out string bindingDescription)
        {
            if (sourceClip == null)
            {
                throw new ArgumentNullException(nameof(sourceClip));
            }

            if (sourceClip.length <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceClip), "源动画时长必须大于 0");
            }

            if (targetDurationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetDurationSeconds),
                    "目标动作时长必须大于 0");
            }

            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "采样帧率必须大于 0");
            }

            if (!TryGetRootDisplacementCurve(
                    sourceClip,
                    out AnimationCurve sourceCurve,
                    out bindingDescription))
            {
                throw new InvalidOperationException(
                    "源动画没有可用的根位移 Z 曲线 请确认 Root Transform Position XZ 没有烘焙到姿势");
            }

            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sourceClip.length * sampleRate));
            Keyframe[] keys = new Keyframe[sampleCount + 1];
            float sourceStartValue = sourceCurve.Evaluate(0f);
            float displacementSign = invertDisplacement ? -1f : 1f;

            for (int index = 0; index <= sampleCount; index++)
            {
                float normalizedTime = (float)index / sampleCount;
                float sourceTime = sourceClip.length * normalizedTime;
                float targetTime = targetDurationSeconds * normalizedTime;
                float displacement =
                    (sourceCurve.Evaluate(sourceTime) - sourceStartValue) * displacementSign;

                keys[index] = new Keyframe(targetTime, displacement);
            }

            AnimationCurve bakedCurve = new AnimationCurve(keys)
            {
                preWrapMode = WrapMode.ClampForever,
                postWrapMode = WrapMode.ClampForever
            };

            if (linearTangents)
            {
                SetLinearTangents(bakedCurve);
            }

            return bakedCurve;
        }

        internal static void WriteCurve(
            CharacterActionAsset targetAction,
            AnimationCurve bakedCurve)
        {
            if (targetAction == null)
            {
                throw new ArgumentNullException(nameof(targetAction));
            }

            if (bakedCurve == null)
            {
                throw new ArgumentNullException(nameof(bakedCurve));
            }

            Undo.RegisterCompleteObjectUndo(targetAction, "烘焙角色位移曲线");

            SerializedObject serializedAction = new SerializedObject(targetAction);
            SerializedProperty displacementCurveProperty =
                serializedAction.FindProperty("_zDisplacementCurve");

            if (displacementCurveProperty == null)
            {
                throw new InvalidOperationException(
                    "目标动作没有找到 _zDisplacementCurve 序列化字段");
            }

            displacementCurveProperty.animationCurveValue = bakedCurve;
            serializedAction.ApplyModifiedProperties();

            EditorUtility.SetDirty(targetAction);
            AssetDatabase.SaveAssetIfDirty(targetAction);
        }

        internal static bool TryGetRootDisplacementCurve(
            AnimationClip sourceClip,
            out AnimationCurve curve,
            out string bindingDescription)
        {
            if (sourceClip == null)
            {
                throw new ArgumentNullException(nameof(sourceClip));
            }

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

            if (TryFindBinding(bindings, IsRootTzBinding, out EditorCurveBinding rootTzBinding))
            {
                curve = AnimationUtility.GetEditorCurve(sourceClip, rootTzBinding);
                bindingDescription = rootTzBinding.propertyName;
                return curve != null;
            }

            if (TryFindBinding(
                    bindings,
                    IsRootTransformPositionZBinding,
                    out EditorCurveBinding rootTransformBinding))
            {
                curve = AnimationUtility.GetEditorCurve(sourceClip, rootTransformBinding);
                bindingDescription =
                    $"{rootTransformBinding.path} {rootTransformBinding.propertyName}";
                return curve != null;
            }

            curve = null;
            bindingDescription = string.Empty;
            return false;
        }

        private static bool TryFindBinding(
            IReadOnlyList<EditorCurveBinding> bindings,
            Func<EditorCurveBinding, bool> predicate,
            out EditorCurveBinding binding)
        {
            for (int index = 0; index < bindings.Count; index++)
            {
                if (predicate(bindings[index]))
                {
                    binding = bindings[index];
                    return true;
                }
            }

            binding = default;
            return false;
        }

        private static bool IsRootTzBinding(EditorCurveBinding binding)
        {
            return string.IsNullOrEmpty(binding.path) &&
                   binding.propertyName == RootTzPropertyName;
        }

        private static bool IsRootTransformPositionZBinding(EditorCurveBinding binding)
        {
            return binding.type == typeof(Transform) &&
                   binding.propertyName == LocalPositionZPropertyName &&
                   HasSupportedRootNodeName(binding.path);
        }

        private static bool HasSupportedRootNodeName(string bindingPath)
        {
            int separatorIndex = bindingPath.LastIndexOf('/');
            string nodeName = separatorIndex >= 0
                ? bindingPath.Substring(separatorIndex + 1)
                : bindingPath;

            return nodeName == "Root" || nodeName == "Bip001";
        }

        private static void SetLinearTangents(AnimationCurve curve)
        {
            for (int index = 0; index < curve.length; index++)
            {
                AnimationUtility.SetKeyLeftTangentMode(
                    curve,
                    index,
                    AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(
                    curve,
                    index,
                    AnimationUtility.TangentMode.Linear);
            }
        }
    }

    internal sealed class CharacterDisplacementCurveBakerWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ZZZ/Animation/烘焙角色位移曲线";

        [SerializeField] private AnimationClip _sourceClip;
        [SerializeField] private CharacterActionAsset _targetAction;
        [SerializeField] private int _sampleRate = 30;
        [SerializeField] private bool _invertDisplacement;
        [SerializeField] private bool _linearTangents = true;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            CharacterDisplacementCurveBakerWindow window =
                GetWindow<CharacterDisplacementCurveBakerWindow>();

            window.titleContent = new GUIContent("角色位移曲线烘焙");
            window.minSize = new Vector2(520f, 380f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("输入与输出", EditorStyles.boldLabel);

            _sourceClip = (AnimationClip)EditorGUILayout.ObjectField(
                new GUIContent("源动画", "请选择原始 FBX 中保留根位移的 AnimationClip"),
                _sourceClip,
                typeof(AnimationClip),
                false);

            _targetAction = (CharacterActionAsset)EditorGUILayout.ObjectField(
                new GUIContent("目标动作", "位移曲线将写入该动作资产"),
                _targetAction,
                typeof(CharacterActionAsset),
                false);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("采样设置", EditorStyles.boldLabel);

            _sampleRate = EditorGUILayout.IntSlider("采样帧率", _sampleRate, 1, 120);
            _invertDisplacement = EditorGUILayout.ToggleLeft(
                new GUIContent("反转前进方向", "当源动画的前进方向为负 Z 时启用"),
                _invertDisplacement);
            _linearTangents = EditorGUILayout.ToggleLeft(
                new GUIContent("使用 Linear 切线", "保留采样点之间的线性位移"),
                _linearTangents);

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "工具优先读取 RootT z 曲线 其次读取 Root 或 Bip001 的本地 Z 位移曲线" +
                "输出为动作逻辑时间上的累计位移曲线" +
                "源动画不会被修改",
                MessageType.Info);

            string validationMessage = GetValidationMessage();
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }
            else
            {
                DrawPreview();
            }

            using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(validationMessage)))
            {
                if (GUILayout.Button("烘焙并写入目标动作", GUILayout.Height(30f)))
                {
                    Bake();
                }
            }
        }

        private void DrawPreview()
        {
            CharacterDisplacementCurveBaker.TryGetRootDisplacementCurve(
                _sourceClip,
                out AnimationCurve sourceCurve,
                out string bindingDescription);

            float startValue = sourceCurve.Evaluate(0f);
            float endValue = sourceCurve.Evaluate(_sourceClip.length);
            float netDisplacement = endValue - startValue;

            if (_invertDisplacement)
            {
                netDisplacement = -netDisplacement;
            }

            EditorGUILayout.HelpBox(
                $"位移绑定 {bindingDescription} 源动画时长 {_sourceClip.length:F3} " +
                $"目标动作时长 {_targetAction.DurationSeconds:F3} " +
                $"预计单次位移 {netDisplacement:F3}",
                Mathf.Abs(netDisplacement) < 0.0001f
                    ? MessageType.Warning
                    : MessageType.None);
        }

        private void Bake()
        {
            AnimationCurve bakedCurve = CharacterDisplacementCurveBaker.BuildCurve(
                _sourceClip,
                _targetAction.DurationSeconds,
                _sampleRate,
                _invertDisplacement,
                _linearTangents,
                out string bindingDescription);

            CharacterDisplacementCurveBaker.WriteCurve(_targetAction, bakedCurve);

            ShowNotification(
                new GUIContent(
                    $"烘焙完成 关键帧数 {bakedCurve.length} 绑定 {bindingDescription}"));
            Repaint();
        }

        private string GetValidationMessage()
        {
            if (_sourceClip == null)
            {
                return "请指定源动画";
            }

            if (_targetAction == null)
            {
                return "请指定目标动作";
            }

            if (_sourceClip.length <= 0f)
            {
                return "源动画时长必须大于 0";
            }

            if (_targetAction.DurationSeconds <= 0f)
            {
                return "目标动作时长必须大于 0";
            }

            if (!CharacterDisplacementCurveBaker.TryGetRootDisplacementCurve(
                    _sourceClip,
                    out _,
                    out _))
            {
                return "源动画没有可用的根位移 Z 曲线 请检查 Root Transform Position XZ 的 Bake Into Pose 设置";
            }

            return string.Empty;
        }
    }
}
