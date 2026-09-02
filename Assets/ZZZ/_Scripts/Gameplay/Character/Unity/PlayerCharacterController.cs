using System;
using System.Collections.Generic;

using UnityEngine;

using GamePlay.Character.Public;
using SPFramework;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerCharacterController : MonoBehaviour, IPlayerCharacterService
    {
        [SerializeField]
        private CharacterInfoAsset _characterInfoAsset;
        [SerializeField]
        private CharacterActionSetAsset _characterActionSetAsset;

        private CharacterController _characterController;
        private Animator _animator;
        private CharacterInfoCalculator _characterInfoCalculator;
        private CharacterInfo _characterInfo;
        private CharacterActionState _characterActionState;
        private PlayerCharacterCoordinator _coordinator;
        private PlayerCharacterServiceRouter _router;
        private int _entityId = -1;
        private bool _isInitialized;

        public int EntityId => _entityId;

        public CharacterInfo CharacterInfo => _characterInfo;

        private void Awake()
        {
            if (_characterInfoAsset == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterController)} 必须配置 {nameof(_characterInfoAsset)}");
            }

            if (_characterActionSetAsset == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterController)} 必须配置 {nameof(_characterActionSetAsset)}");
            }

            _characterInfoAsset.Validate();
            _characterActionSetAsset.BuildRuntimeLookups(
                out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
                out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId);
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            CharacterPositionDriver positionDriver =
                new CharacterPositionDriver(_characterController);
            CharacterRotationDriver rotationDriver =
                new CharacterRotationDriver(transform);
            CharacterAnimationPlayer animationPlayer =
                new CharacterAnimationPlayer(_animator);
            CharacterActionArbiter arbiter =
                new CharacterActionArbiter(actionsById, linksBySourceActionId);
            CharacterActionTransition transition =
                new CharacterActionTransition(actionsById);
            _coordinator = new PlayerCharacterCoordinator(
                arbiter,
                transition,
                positionDriver,
                rotationDriver,
                animationPlayer);
            _characterInfoCalculator = new CharacterInfoCalculator();
            _characterActionState = CharacterActionState.CreateInitial(
                _characterActionSetAsset.InitialAction.ActionId);
            _isInitialized = true;
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterController)} 尚未完成初始化");
            }

            if (!ServiceHub.TryGet<IPlayerCharacterService>(
                    out IPlayerCharacterService playerCharacterService)
                || !(playerCharacterService is PlayerCharacterServiceRouter router))
            {
                throw new InvalidOperationException(
                    $"{nameof(IPlayerCharacterService)} 未注册唯一路由 不能启用 {nameof(PlayerCharacterController)}");
            }

            _router = router;
            _entityId = _router.RegisterRuntimeUnit(this);
            if (_characterInfo == null || _characterInfo.EntityId != _entityId)
            {
                _characterInfo = _characterInfoCalculator.CalculateInitialInfo(
                    _characterInfoAsset,
                    _entityId);
            }
        }

        private void OnDisable()
        {
            if (_router != null && !_router.IsDisposed)
            {
                _router.SuspendRuntimeUnit(this);
            }
        }

        private void OnDestroy()
        {
            if (_router != null && !_router.IsDisposed)
            {
                _router.UnregisterRuntimeUnit(this);
            }

            _router = null;

            if (_coordinator != null)
            {
                _coordinator.Dispose();
                _coordinator = null;
            }
        }

        public void CharacterUpdate()
        {
            _coordinator.Tick(ref _characterActionState, Time.deltaTime);
        }

        public void EnterField(int characterEntityId, Transform characterTransform)
        {
            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            if (characterEntityId != _entityId)
            {
                throw new InvalidOperationException(
                    $"角色实体 ID 不匹配 期望 {_entityId} 实际 {characterEntityId}");
            }

            gameObject.SetActive(true);
            transform.SetPositionAndRotation(
                characterTransform.position,
                characterTransform.rotation);
            _characterActionState = CharacterActionState.CreateInitial(
                _characterActionSetAsset.InitialAction.ActionId);
            _coordinator.Reset(
                _characterActionSetAsset.InitialAction,
                ref _characterActionState);
            _characterActionState.Fact = _characterActionState.Fact.MarkEnterField();
        }

        public void ExitField()
        {
            _characterActionState.Fact = _characterActionState.Fact.MarkExitField();
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}
