using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace GamePlay.Editor
{
    [Flags]
    internal enum InPlacePositionAxes
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2
    }

    internal static class InPlaceAnimationClipConverter
    {
        internal static int Convert(
            AnimationClip sourceClip,
            AnimationClip targetClip,
            InPlacePositionAxes axes,
            Vector3 constantPosition)
        {
            if (sourceClip == null || targetClip == null ||
                sourceClip == targetClip || axes == InPlacePositionAxes.None)
            {
                throw new ArgumentException(
                    "Source and target clips must be different, and at least one position axis must be selected.");
            }

            EditorCurveBinding[] targetBindings = AnimationUtility
                .GetCurveBindings(sourceClip)
                .Where(binding => ShouldFlatten(binding, axes))
                .ToArray();

            string targetPath = AssetDatabase.GetAssetPath(targetClip);
            bool isWritableStandaloneClip = AssetDatabase.IsMainAsset(targetClip) &&
                                            targetPath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase);

            if (!isWritableStandaloneClip || targetBindings.Length == 0)
            {
                throw new InvalidOperationException(
                    !isWritableStandaloneClip
                        ? "The target must be a standalone .anim main asset. FBX sub-clips cannot be overwritten."
                        : "The source clip has no selected position curves on RootT, Bip001, or Root.");
            }

            string targetName = targetClip.name;
            HideFlags targetHideFlags = targetClip.hideFlags;

            Undo.RegisterCompleteObjectUndo(targetClip, "Convert AnimationClip To In-Place");
            EditorUtility.CopySerialized(sourceClip, targetClip);

            targetClip.name = targetName;
            targetClip.hideFlags = targetHideFlags;

            foreach (EditorCurveBinding binding in targetBindings)
            {
                InPlacePositionAxes axis = GetAxis(binding);
                float constantValue = GetConstantValue(axis, constantPosition);
                AnimationCurve constantCurve = new AnimationCurve(
                    new Keyframe(0f, constantValue),
                    new Keyframe(targetClip.length, constantValue));

                AnimationUtility.SetEditorCurve(targetClip, binding, constantCurve);
            }

            EditorUtility.SetDirty(targetClip);
            AssetDatabase.SaveAssetIfDirty(targetClip);
            return targetBindings.Length;
        }

        internal static bool IsStandaloneAnimationAsset(AnimationClip clip)
        {
            if (clip == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(clip);
            return AssetDatabase.IsMainAsset(clip) &&
                   assetPath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldFlatten(
            EditorCurveBinding binding,
            InPlacePositionAxes axes)
        {
            InPlacePositionAxes axis = GetAxis(binding);
            return IsSupportedRootTranslation(binding) && (axes & axis) != 0;
        }

        private static bool IsSupportedRootTranslation(EditorCurveBinding binding)
        {
            bool isRootT = binding.propertyName == "RootT.x" ||
                           binding.propertyName == "RootT.y" ||
                           binding.propertyName == "RootT.z";

            bool isLocalPosition = binding.propertyName == "m_LocalPosition.x" ||
                                   binding.propertyName == "m_LocalPosition.y" ||
                                   binding.propertyName == "m_LocalPosition.z";

            return isRootT || (isLocalPosition && HasSupportedNodeName(binding.path));
        }

        private static bool HasSupportedNodeName(string bindingPath)
        {
            int separatorIndex = bindingPath.LastIndexOf('/');
            string nodeName = separatorIndex >= 0
                ? bindingPath.Substring(separatorIndex + 1)
                : bindingPath;

            return nodeName == "Bip001" || nodeName == "Root";
        }

        private static InPlacePositionAxes GetAxis(EditorCurveBinding binding)
        {
            if (binding.propertyName.EndsWith(".x", StringComparison.Ordinal))
            {
                return InPlacePositionAxes.X;
            }

            if (binding.propertyName.EndsWith(".y", StringComparison.Ordinal))
            {
                return InPlacePositionAxes.Y;
            }

            if (binding.propertyName.EndsWith(".z", StringComparison.Ordinal))
            {
                return InPlacePositionAxes.Z;
            }

            return InPlacePositionAxes.None;
        }

        private static float GetConstantValue(
            InPlacePositionAxes axis,
            Vector3 constantPosition)
        {
            switch (axis)
            {
                case InPlacePositionAxes.X:
                    return constantPosition.x;
                case InPlacePositionAxes.Y:
                    return constantPosition.y;
                case InPlacePositionAxes.Z:
                    return constantPosition.z;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
    }

    internal sealed class InPlaceAnimationClipConverterWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ZZZ/Animation/转原地动画";

        [SerializeField] private AnimationClip _sourceClip;
        [SerializeField] private AnimationClip _targetClip;

        [SerializeField] private bool _processX = true;
        [SerializeField] private float _constantX;

        [SerializeField] private bool _processY;
        [SerializeField] private float _constantY;

        [SerializeField] private bool _processZ = true;
        [SerializeField] private float _constantZ;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            InPlaceAnimationClipConverterWindow window =
                GetWindow<InPlaceAnimationClipConverterWindow>();

            window.titleContent = new GUIContent("In-Place 动画转换");
            window.minSize = new Vector2(480f, 310f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("AnimationClip", EditorStyles.boldLabel);

            _sourceClip = (AnimationClip)EditorGUILayout.ObjectField(
                new GUIContent("源动画", "源动画不会被修改，可以是 FBX 中的 AnimationClip 子资产。"),
                _sourceClip,
                typeof(AnimationClip),
                false);

            _targetClip = (AnimationClip)EditorGUILayout.ObjectField(
                new GUIContent("目标动画", "将被完整覆盖的独立 .anim 资产，其 GUID 与文件名保持不变。"),
                _targetClip,
                typeof(AnimationClip),
                false);

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "目标动画的全部内容会先复制自源动画，然后只将 RootT、Bip001、Root 的所选位置轴改为常量。" +
                "其他位置轴、旋转、身体与武器骨骼曲线保持不变。该操作支持 Undo。",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("位置轴与常量", EditorStyles.boldLabel);
            DrawAxisOption("X", ref _processX, ref _constantX);
            DrawAxisOption("Y", ref _processY, ref _constantY);
            DrawAxisOption("Z", ref _processZ, ref _constantZ);

            EditorGUILayout.Space(8f);
            string validationMessage = GetValidationMessage();

            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(validationMessage)))
            {
                if (GUILayout.Button("转换并覆盖目标动画", GUILayout.Height(30f)))
                {
                    ConvertClip();
                }
            }
        }

        private static void DrawAxisOption(
            string axisName,
            ref bool enabled,
            ref float constantValue)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                enabled = EditorGUILayout.ToggleLeft(axisName, enabled, GUILayout.Width(45f));

                using (new EditorGUI.DisabledScope(!enabled))
                {
                    constantValue = EditorGUILayout.FloatField("常量", constantValue);
                }
            }
        }

        private void ConvertClip()
        {
            InPlacePositionAxes axes = GetSelectedAxes();
            Vector3 constantPosition = new Vector3(_constantX, _constantY, _constantZ);
            int convertedCurveCount = InPlaceAnimationClipConverter.Convert(
                _sourceClip,
                _targetClip,
                axes,
                constantPosition);

            ShowNotification(new GUIContent($"转换完成：{convertedCurveCount} 条曲线"));
            Debug.Log(
                $"Converted '{_sourceClip.name}' to '{_targetClip.name}': " +
                $"{convertedCurveCount} root position curves, axes={axes}, " +
                $"constant=({constantPosition.x}, {constantPosition.y}, {constantPosition.z}).",
                _targetClip);
        }

        private string GetValidationMessage()
        {
            if (_sourceClip == null || _targetClip == null)
            {
                return "请指定源动画和目标动画。";
            }

            if (_sourceClip == _targetClip)
            {
                return "源动画与目标动画不能是同一个资产。";
            }

            if (!InPlaceAnimationClipConverter.IsStandaloneAnimationAsset(_targetClip))
            {
                return "目标动画必须是项目中的独立 .anim 主资产，不能使用 FBX 内的子动画。";
            }

            if (GetSelectedAxes() == InPlacePositionAxes.None)
            {
                return "至少选择一个需要转为常量的位置轴。";
            }

            return string.Empty;
        }

        private InPlacePositionAxes GetSelectedAxes()
        {
            InPlacePositionAxes axes = InPlacePositionAxes.None;

            if (_processX)
            {
                axes |= InPlacePositionAxes.X;
            }

            if (_processY)
            {
                axes |= InPlacePositionAxes.Y;
            }

            if (_processZ)
            {
                axes |= InPlacePositionAxes.Z;
            }

            return axes;
        }
    }

}
