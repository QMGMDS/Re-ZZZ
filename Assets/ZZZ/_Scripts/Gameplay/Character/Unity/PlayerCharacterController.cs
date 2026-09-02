using System;

using UnityEngine;

using GamePlay.Character.Public;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerCharacterController : MonoBehaviour, IPlayerCharacterService
    {
        // 依赖组件
        private CharacterController _characterController;
        private Animator _animator;

        // 依赖配置
        [SerializeField]
        private CharacterActionSetAsset _characterActionSetAsset;
        [SerializeField]
        private CharacterInfoAsset _characterInfoAsset;

        private CharacterInfo _characterInfo;
        private CharacterActionState _characterActionState;

        private PlayerCharacterCoordinator _coordinator;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (_characterInfoAsset == null || _characterActionSetAsset == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerCharacterController)} 检查配置");
            }

            _coordinator = new PlayerCharacterCoordinator(_characterActionSetAsset, _characterController, transform, _animator);

        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnDestroy()
        {
            _coordinator.Dispose();
            _coordinator = null;
        }

        #region 服务接口

        /// <inheritdoc/>
        public void CharacterUpdate()
        {
            _coordinator.Tick(ref _characterActionState, Time.deltaTime);
        }

        /// <inheritdoc/>
        public void EnterField(int characterEntityId, Transform characterTransform)
        {
        }

        /// <inheritdoc/>
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
