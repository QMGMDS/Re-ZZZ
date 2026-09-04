using System;

using UnityEngine;

namespace GamePlay.AI
{
    [CreateAssetMenu(fileName = "AIConfig", menuName = "ZZZ/AI/敌人 AI 配置")]
    public sealed class AIConfigAsset : ScriptableObject
    {
        [Header("视野范围")]
        [SerializeField, Min(0f), Tooltip("视野半径")]
        private float _visionRadius;
        [SerializeField, Range(0f, 360f), Tooltip("视野完整扇形角度")]
        private float _visionAngleDegrees;

        [Header("攻击范围")]
        [SerializeField, Min(0f), Tooltip("攻击半径")]
        private float _attackRadius;
        [SerializeField, Range(0f, 360f), Tooltip("攻击完整扇形角度")]
        private float _attackAngleDegrees;

        public float VisionRadius => _visionRadius;
        public float VisionAngleDegrees => _visionAngleDegrees;
        public float AttackRadius => _attackRadius;
        public float AttackAngleDegrees => _attackAngleDegrees;
    }
}
