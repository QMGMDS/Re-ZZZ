using System;

using UnityEngine;

using GamePlay.Character.Contract;
using SPFramework;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色动作控制 Root Mono
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterActionController : MonoBehaviour, ICharacterUpdateTarget
    {
        // 必要组件
        private CharacterController _characterController;
        private Animator _animator;

        [Header("自定义配置")]
        [SerializeField, Tooltip("本角色的信息资产")]
        private CharacterInfoAsset _characterInfoAsset;
        [SerializeField, Tooltip("本角色的动作资产集合")]
        private CharacterActionSetAsset _actionSet;

        private ICharacterModule _characterModule;

        private CharacterInfoRuntime _runtime;
        private int _entityId;

        public CharacterInfoRuntime Runtime => _runtime;
        public int EntityId => _entityId;

        // 仲裁器
        private CharacterActionArbiter _arbiter;
        // 过渡器
        private CharacterActionTransition _transition;
        // 位移器
        private CharacterDisplacementDriver _displacementDriver;
        // 旋转器
        private CharacterRotationDriver _rotationDriver;
        // 动画驱动器
        private CharacterAnimationDriver _animationDriver;

        // 当前动作
        private CharacterActionAsset _currentAction;
        private float _logicalProgressSeconds;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (_characterInfoAsset == null
                || _actionSet == null)
            {
                throw new InvalidOperationException("检查配置");
            }

            _runtime = new CharacterInfoRuntime(_characterInfoAsset);

            _actionSet.BuildRuntimeLookups(out var actionsById, out var linksBySourceActionId);

            _arbiter = new CharacterActionArbiter(actionsById, linksBySourceActionId);
            _transition = new CharacterActionTransition();

            _displacementDriver = new CharacterDisplacementDriver(_characterController);
            _rotationDriver = new CharacterRotationDriver(transform);
            _animationDriver = new CharacterAnimationDriver(_animator, linksBySourceActionId);

            // 默认动作
            _currentAction = _actionSet.DefaultAction;
        }

        private void OnEnable()
        {
            if (!ServiceHub.TryGet<ICharacterModule>(out ICharacterModule characterModule))
            {
                throw new InvalidOperationException(
                    $"{nameof(ICharacterModule)} 未注册 不能启用 {nameof(CharacterActionController)}");
            }

            _entityId = characterModule.Register(this, _runtime);
            _characterModule = characterModule;
        }

        private void OnDisable()
        {
            _characterModule.Unregister(this);
            _characterModule = null;
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            CharacterFact fact = _runtime.Fact;
            float currentActionProgress = _transition.GetNormalizedProgress(_currentAction);

            // 裁决
            CharacterActionAsset targetAction = _arbiter.TrySwitch(
                _currentAction.Id,
                currentActionProgress,
                _runtime.Intention,
                fact);
            // 过渡
            _logicalProgressSeconds = _transition.Tick(
                targetAction,
                ref _currentAction,
                tickDeltaSeconds);

            // 位移
            _displacementDriver.Evaluate(
                _currentAction,
                _logicalProgressSeconds,
                _runtime.MoveDirection);
            // 旋转
            _rotationDriver.Evaluate(
                _currentAction,
                _runtime.MoveDirection,
                tickDeltaSeconds);
        }

        /// <inheritdoc/>
        public void RenderUpdate(float deltaTimeSeconds)
        {
            _animationDriver.Evaluate(
                _currentAction,
                _logicalProgressSeconds,
                deltaTimeSeconds);
        }

        /// <summary>
        /// 写入本次逻辑 Tick 的角色运行时数据
        /// </summary>
        public void WriteRuntimeData(InputCharacterData inputCharacterData)
        {
            _runtime.Intention = inputCharacterData.Intention;
            _runtime.MoveDirection = inputCharacterData.MoveInput;
        }

        private void OnDestroy()
        {
            if (_animationDriver != null)
            {
                _animationDriver.Dispose();
            }
        }
    }
}
