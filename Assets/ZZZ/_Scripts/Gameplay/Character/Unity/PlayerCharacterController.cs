using System;

using UnityEngine;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        // 依赖组件
        private Animator _animator;
        private CharacterController _characterController;

        // 依赖配置
        [SerializeField]
        private CharacterActionSetAsset _characterActionSetAsset;
        [SerializeField]
        private CharacterInfoAsset _characterInfoAsset;

        // 角色信息
        private CharacterInfo _characterInfo;

        // 依赖逻辑模型
        private PlayerCharacterCoordinator _coordinator;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (_characterInfoAsset == null || _characterActionSetAsset == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerCharacterController)} 检查配置");
            }

            _coordinator = new PlayerCharacterCoordinator(_characterActionSetAsset, _animator, _characterController);
        }

        /// <summary>
        /// 销毁该角色
        /// </summary>
        private void OnDestroy()
        {
            _coordinator.Dispose();
            _coordinator = null;
        }

        #region 模块内部调用接口

        /// <summary>
        /// 更新该角色
        /// </summary>
        public void CharacterUpdate()
        {
            _coordinator.Tick();
        }

        /// <summary>
        /// 写入该角色的当前意图
        /// </summary>
        public void SetIntention(CharacterIntention intention, Vector2 moveDirectionInWorld)
        {
            _coordinator.SetIntention(intention, moveDirectionInWorld);
        }

        /// <summary>
        /// 初始化该角色信息
        /// </summary>
        public void InitializeCharacterInfo(int assignedEntityId)
        {
            _characterInfo = new CharacterInfo(_characterInfoAsset, assignedEntityId);
        }

        /// <summary>
        /// 命令该角色进场
        /// </summary>
        public void EnterField()
        {
        }

        /// <summary>
        /// 命令该角色离场
        /// </summary>
        public void ExitField()
        {
        }

        #endregion

        #region 动画事件

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}
