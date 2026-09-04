using System;

using UnityEngine;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public sealed class EnemyCharacterController : MonoBehaviour
    {
        // 依赖组件
        private Animator _animator;
        private CharacterController _characterController;

        // 依赖配置
        [SerializeField]
        private CharacterActionSetAsset _characterActionSetAsset;

        // 依赖逻辑模型
        private EnemyCharacterCoordinator _coordinator;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (_characterActionSetAsset == null)
            {
                throw new InvalidOperationException($"{nameof(EnemyCharacterController)} 检查配置");
            }

            _coordinator = new EnemyCharacterCoordinator(_characterActionSetAsset, _animator, _characterController);
        }

        private void OnDestroy()
        {
            _coordinator.Dispose();
            _coordinator = null;
        }

        #region 模块内部调用接口

        /// <summary>
        /// 更新该敌人角色
        /// </summary>
        public void CharacterUpdate()
        {
            _coordinator.Tick();
        }

        /// <summary>
        /// 写入该敌人角色的当前意图
        /// </summary>
        public void SetIntention(CharacterIntention intention)
        {
            _coordinator.SetIntention(intention);
        }

        /// <summary>
        /// 写入该敌人角色的世界空间移动方向
        /// </summary>
        public void SetMoveDirectionInWorld(Vector2 moveDirectionInWorld)
        {
            _coordinator.SetMoveDirectionInWorld(moveDirectionInWorld);
        }

        #endregion
    }
}
