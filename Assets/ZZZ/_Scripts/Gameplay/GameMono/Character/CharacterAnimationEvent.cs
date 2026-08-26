using System;

using UnityEngine;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 统一接收角色动画事件
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterAnimationEvent : MonoBehaviour
    {
        [Header("角色动画事件依赖")]
        [SerializeField, Tooltip("负责执行攻击检测的碰撞体")]
        private CharacterAttackCollider _attackCollider;

        private void Awake()
        {
            if (_attackCollider == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterAnimationEvent)} 要求必须分配 {nameof(_attackCollider)}");
            }
        }

        /// <summary>
        /// 开启一次攻击检测窗口
        /// </summary>
        public void OpenAttackDetectionWindow()
        {
            _attackCollider.OpenAttackDetectionWindow();
        }

        /// <summary>
        /// 关闭一次攻击检测窗口
        /// </summary>
        public void CloseAttackDetectionWindow()
        {
            _attackCollider.CloseAttackDetectionWindow();
        }
    }
}
