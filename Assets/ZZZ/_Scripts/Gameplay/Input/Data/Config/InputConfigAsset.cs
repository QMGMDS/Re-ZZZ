using System;

using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Input
{
    /// <summary>
    /// 输入模块的静态配置资产
    /// </summary>
    [CreateAssetMenu(fileName = "InputConfig", menuName = "ZZZ/输入/输入配置资产")]
    public sealed class InputConfigAsset : ScriptableObject
    {
        [Header("输入绑定")]
        [SerializeField, Tooltip("角色移动输入引用")]
        private InputActionReference _moveInputReference;
        [SerializeField, Tooltip("角色攻击输入引用")]
        private InputActionReference _attackInputReference;
        [SerializeField, Tooltip("角色闪避输入引用")]
        private InputActionReference _evadeInputReference;
        [SerializeField, Tooltip("角色技能输入引用")]
        private InputActionReference _skillInputReference;
        [SerializeField, Tooltip("角色终结技输入引用")]
        private InputActionReference _ultimateInputReference;
        [SerializeField, Tooltip("角色切换输入引用")]
        private InputActionReference _switchInputReference;

        [Header("输入处理")]
        [SerializeField, Min(0f), Tooltip("方向切换时的移动输入空窗容错秒数")]
        private float _moveInputGapToleranceSeconds = 0.05f;

        public InputActionReference MoveInputReference => _moveInputReference;
        public InputActionReference AttackInputReference => _attackInputReference;
        public InputActionReference EvadeInputReference => _evadeInputReference;
        public InputActionReference SkillInputReference => _skillInputReference;
        public InputActionReference UltimateInputReference => _ultimateInputReference;
        public InputActionReference SwitchInputReference => _switchInputReference;
        public float MoveInputGapToleranceSeconds => _moveInputGapToleranceSeconds;

        internal void Validate()
        {
            ValidateInputReference(_moveInputReference, nameof(_moveInputReference));
            ValidateInputReference(_attackInputReference, nameof(_attackInputReference));
            ValidateInputReference(_evadeInputReference, nameof(_evadeInputReference));
            ValidateInputReference(_skillInputReference, nameof(_skillInputReference));
            ValidateInputReference(_ultimateInputReference, nameof(_ultimateInputReference));
            ValidateInputReference(_switchInputReference, nameof(_switchInputReference));

            if (_moveInputGapToleranceSeconds < 0f
                || float.IsNaN(_moveInputGapToleranceSeconds)
                || float.IsInfinity(_moveInputGapToleranceSeconds))
            {
                throw new InvalidOperationException(
                    $"{nameof(InputConfigAsset)} 的 {nameof(_moveInputGapToleranceSeconds)} 必须是非负有限数值");
            }
        }

        private static void ValidateInputReference(InputActionReference inputReference, string fieldName)
        {
            if (inputReference == null || inputReference.action == null)
            {
                throw new InvalidOperationException($"{nameof(InputConfigAsset)} 的 {fieldName} 必须分配有效的 InputActionReference");
            }
        }
    }
}
