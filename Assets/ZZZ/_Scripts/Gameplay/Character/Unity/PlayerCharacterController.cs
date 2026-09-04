using System;

using UnityEngine;

using SPFramework;
using GamePlay.Character.Public;

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

        #region 可调用接口

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
        /// 注册该角色信息
        /// </summary>
        public void RegisterCharacterInfo()
        {
            if (!ServiceHub.TryGet<ICharacterInfoRegistryService>(out ICharacterInfoRegistryService characterInfoRegistryService))
            {
                throw new InvalidOperationException($"{nameof(PlayerCharacterController)} 未获取到 {nameof(ICharacterInfoRegistryService)}");
            }

            _characterInfo = characterInfoRegistryService.RegisterCharacterInfo(_characterInfoAsset);
        }

        /// <summary>
        /// 激活初始角色物体
        /// </summary>
        public void ActivateInitial()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 命令该角色进场
        /// </summary>
        public void EnterField(Transform characterTransform)
        {
            transform.SetPositionAndRotation(characterTransform.position, characterTransform.rotation);
            gameObject.SetActive(true);
            _coordinator.MarkEnterField();
        }

        /// <summary>
        /// 命令该角色离场
        /// </summary>
        public Transform ExitField()
        {
            _coordinator.MarkExitField();
            return transform;
        }

        /// <summary>
        /// 角色属性
        /// </summary>
        public CharacterInfo CharacterInfo => _characterInfo;

        #endregion

        #region 动画事件

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}
