using System;

using UnityEngine;

namespace GamePlay.Character
{
    public enum ActionDirectionMode
    {
        LiveMoveDirection = 0,
        CaptureMoveDirectionOnEnter = 1,
        CaptureFacingDirectionOnEnter = 2
    }

    [CreateAssetMenu(fileName = "CharacterAction", menuName = "ZZZ/角色/动作单资产")]
    public sealed class CharacterActionAsset : ScriptableObject
    {
        [SerializeField]
        private string _actionId;
        [SerializeField, Min(0f)]
        private float _durationSeconds;
        [SerializeField]
        private AnimationClip _animationClip;
        [SerializeField]
        private ActionDirectionMode _actionDirectionMode;
        [SerializeField, Min(0f)]
        private float _maxRotationSpeedDegreesPerSecond;
        [SerializeField]
        private AnimationCurve _cumulativeForwardDisplacement;

        public string ActionId => _actionId;

        public float DurationSeconds => _durationSeconds;

        public AnimationClip AnimationClip => _animationClip;

        public ActionDirectionMode ActionDirectionMode => _actionDirectionMode;

        public float MaxRotationSpeedDegreesPerSecond => _maxRotationSpeedDegreesPerSecond;

        public AnimationCurve CumulativeForwardDisplacement => _cumulativeForwardDisplacement;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_actionId))
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionAsset)} 的 {nameof(ActionId)} 不能为空");
            }

            if (!IsFinitePositive(_durationSeconds))
            {
                throw new InvalidOperationException(
                    $"动作 {_actionId} 的 {nameof(DurationSeconds)} 必须是大于零的有限秒数");
            }

            if (_animationClip == null)
            {
                throw new InvalidOperationException(
                    $"动作 {_actionId} 的 {nameof(AnimationClip)} 不能为空");
            }

            if (!Enum.IsDefined(typeof(ActionDirectionMode), _actionDirectionMode))
            {
                throw new InvalidOperationException(
                    $"动作 {_actionId} 的 {nameof(ActionDirectionMode)} 无效");
            }

            if (!IsFiniteNonNegative(_maxRotationSpeedDegreesPerSecond))
            {
                throw new InvalidOperationException(
                    $"动作 {_actionId} 的 {nameof(MaxRotationSpeedDegreesPerSecond)} 必须是大于等于零的有限速度");
            }

            if (_cumulativeForwardDisplacement == null)
            {
                throw new InvalidOperationException(
                    $"动作 {_actionId} 的 {nameof(CumulativeForwardDisplacement)} 不能为空");
            }
        }

        public float EvaluateCumulativeForwardDisplacement(float logicalProgressSeconds)
        {
            if (!IsFiniteNonNegative(logicalProgressSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(logicalProgressSeconds));
            }

            if (logicalProgressSeconds > _durationSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalProgressSeconds));
            }

            if (_cumulativeForwardDisplacement == null)
            {
                throw new InvalidOperationException(
                    $"动作 {_actionId} 的 {nameof(CumulativeForwardDisplacement)} 不能为空");
            }

            return _cumulativeForwardDisplacement.Evaluate(logicalProgressSeconds);
        }

        private static bool IsFinitePositive(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
        }
    }
}
