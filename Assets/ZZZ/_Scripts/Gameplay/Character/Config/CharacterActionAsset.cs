using System;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 单个角色动作的静态配置
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterAction", menuName = "ZZZ/角色/动作单资产")]
    public sealed class CharacterActionAsset : ScriptableObject
    {
        [SerializeField, Tooltip("动作 ID")]
        private string _id;
        [SerializeField, Min(0f), Tooltip("动作逻辑时长")]
        private float _durationSeconds;
        [SerializeField, Tooltip("动作播放动画片段")]
        private AnimationClip _animationClip;
        [SerializeField, Tooltip("动作位移曲线")]
        private AnimationCurve _zDisplacementCurve;
        [SerializeField, Min(0f), Tooltip("角色旋转最大速度 单位为度/秒")]
        private float _rotationSpeedDegreesPerSecond;
        [SerializeField, Min(0f), Tooltip("攻击开始逻辑时间 单位为秒")]
        private float _attackStartSeconds;
        [SerializeField, Min(0f), Tooltip("攻击结束逻辑时间 单位为秒")]
        private float _attackEndSeconds;

        public string Id => _id;
        public float DurationSeconds => _durationSeconds;
        public AnimationClip AnimationClip => _animationClip;
        public float RotationSpeedDegreesPerSecond => _rotationSpeedDegreesPerSecond;
        public float AttackStartSeconds => _attackStartSeconds;
        public float AttackEndSeconds => _attackEndSeconds;

        /// <summary>
        /// 判断动作在指定逻辑时刻是否处于攻击区间
        /// </summary>
        public bool IsAttackActiveAt(float logicalProgressSeconds)
        {
            return logicalProgressSeconds >= _attackStartSeconds
                && logicalProgressSeconds < _attackEndSeconds;
        }

        /// <summary>
        /// 读取动作在指定逻辑时刻的累计 Z 位移
        /// </summary>
        public float EvaluateZDisplacement(float logicalProgressSeconds)
        {
            if (_zDisplacementCurve == null)
                throw new InvalidOperationException("未配置位移曲线");
            return _zDisplacementCurve.Evaluate(logicalProgressSeconds);
        }
    }
}
