using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 动作运动模式
    /// </summary>
    public enum ActionDirectionMode
    {
        /// <summary>每个逻辑帧都读取最新移动输入，方向会持续变化</summary>
        LiveMoveDirection = 0,
        /// <summary>动作进入瞬间保存当前移动输入方向，动作持续期间不再改变</summary>
        CaptureMoveDirectionOnEnter = 1,
        /// <summary>动作进入瞬间保存角色当前水平朝向，动作持续期间不再改变</summary>
        CaptureFacingDirectionOnEnter = 2
    }

    [CreateAssetMenu(fileName = "CharacterAction", menuName = "ZZZ/角色/动作单资产")]
    public sealed class CharacterActionAsset : ScriptableObject
    {
        [SerializeField, Tooltip("动作ID")]
        private string _actionId;
        [SerializeField, Min(0f), Tooltip("动作逻辑时长")]
        private float _durationSeconds;

        [SerializeField, Tooltip("动作播放动画")]
        private AnimationClip _animationClip;

        [SerializeField, Tooltip("动作运动模式")]
        private ActionDirectionMode _actionDirectionMode;
        [SerializeField, Min(0f), Tooltip("动作最大旋转速度 度/秒")]
        private float _maxRotationSpeedDegreesPerSecond;
        [SerializeField, Tooltip("动作位移曲线")]
        private AnimationCurve _cumulativeForwardDisplacement;

        public string ActionId => _actionId;
        public float DurationSeconds => _durationSeconds;
        public AnimationClip AnimationClip => _animationClip;
        public ActionDirectionMode ActionDirectionMode => _actionDirectionMode;
        public float MaxRotationSpeedDegreesPerSecond => _maxRotationSpeedDegreesPerSecond;
        public AnimationCurve CumulativeForwardDisplacement => _cumulativeForwardDisplacement;
    }
}
