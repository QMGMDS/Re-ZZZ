using System;
using System.Collections.Generic;

using UnityEngine;

using GamePlay.Camera.Public;
using GamePlay.Character.Public;
using GamePlay.Definition;
using GamePlay.Input;
using GamePlay.Input.Public;
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
        private IInputService _inputService;
        private ICameraService _cameraService;
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

        private void Start()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterController)} 尚未完成初始化");
            }

            if (!ServiceHub.TryGet<IInputService>(out _inputService))
            {
                throw new InvalidOperationException(
                    $"{nameof(IInputService)} 未注册 不能启动 {nameof(PlayerCharacterController)}");
            }

            if (!ServiceHub.TryGet<ICameraService>(out _cameraService))
            {
                throw new InvalidOperationException(
                    $"{nameof(ICameraService)} 未注册 不能启动 {nameof(PlayerCharacterController)}");
            }
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterController)} 尚未完成初始化");
            }

            if (!ServiceHub.TryGet<ICharacterService>(
                    out ICharacterService characterService)
                || !(characterService is PlayerCharacterServiceRouter router))
            {
                throw new InvalidOperationException(
                    $"{nameof(ICharacterService)} 未注册唯一路由 不能启用 {nameof(PlayerCharacterController)}");
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
            if (_router != null)
            {
                _router.SuspendRuntimeUnit(this);
            }
        }

        private void OnDestroy()
        {
            if (_router != null)
            {
                _router.UnregisterRuntimeUnit(this);
                _router = null;
            }

            if (_coordinator != null)
            {
                _coordinator.Dispose();
                _coordinator = null;
            }
        }

        public void CharacterUpdate()
        {
            CharacterInputData inputData = _inputService.CharacterInputData;
            _characterActionState.MoveDirectionInWorld = _cameraService.ConvertToWorldCoordinate(inputData.Move);
            _characterActionState.Intention = CreateIntention(inputData, _characterActionState.MoveDirectionInWorld);

            if (inputData.Switch)
            {
                EventBus.Publish(new CharacterSwitchRequestedEvent(transform));
            }

            _coordinator.Tick(ref _characterActionState, Time.deltaTime);
        }

        public void EnterField(Transform characterTransform)
        {
            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
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

        private static CharacterIntention CreateIntention(
            CharacterInputData inputData,
            Vector2 worldMoveDirection)
        {
            return new CharacterIntention(
                ToTrilean(worldMoveDirection.sqrMagnitude > 0f),
                ToTrilean(inputData.Attack),
                ToTrilean(inputData.Evade),
                ToTrilean(inputData.Skill),
                ToTrilean(inputData.Ultimate));
        }

        private static Trilean ToTrilean(bool value)
        {
            return value
                ? Trilean.True
                : Trilean.False;
        }
    }
}
