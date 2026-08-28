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
    public sealed class CharacterActionController : MonoBehaviour, ICharacterUpdateTarget
    {
        [Header("必要组件")]
        [SerializeField, Tooltip("负责角色碰撞移动的 CharacterController")]
        private CharacterController _characterController;
        [SerializeField, Tooltip("显示本角色动画的 Animator")]
        private Animator _animator;
        [SerializeField, Tooltip("角色信息控制器")]
        private CharacterInfoController _characterInfoController;

        [Header("自定义配置")]
        [SerializeField, Tooltip("本角色的动作资产集合")]
        private CharacterActionSetAsset _actionSet;

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
            if (_characterController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionController)} 要求必须分配 {nameof(_characterController)}");
            }

            if (_animator == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionController)} 要求必须分配 {nameof(_animator)}");
            }

            if (_characterInfoController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionController)} 要求必须分配 {nameof(_characterInfoController)}");
            }

            if (_actionSet == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionController)} 要求必须分配 {nameof(_actionSet)}");
            }

            _actionSet.BuildRuntimeLookups(out var actionsById, out var linksBySourceActionId);

            _arbiter = new CharacterActionArbiter(actionsById, linksBySourceActionId);
            _transition = new CharacterActionTransition();

            _displacementDriver = new CharacterDisplacementDriver(_characterController);
            _rotationDriver = new CharacterRotationDriver(transform);
            _animationDriver = new CharacterAnimationDriver(_animator, linksBySourceActionId);

            // 默认动作
            _currentAction = _actionSet.DefaultAction;
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            CharacterInfoRuntime characterInfoRuntime = _characterInfoController.Runtime;
            CharacterFact fact = characterInfoRuntime.Fact;
            float currentActionProgress = _transition.GetNormalizedProgress(_currentAction);

            // 裁决
            CharacterActionAsset targetAction = _arbiter.TrySwitch(
                _currentAction.Id,
                currentActionProgress,
                characterInfoRuntime.Intention,
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
                characterInfoRuntime.MoveDirection);
            // 旋转
            _rotationDriver.Evaluate(
                _currentAction,
                characterInfoRuntime.MoveDirection,
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

        private void OnEnable()
        {
            ModuleSystem.GetModule<ICharacterModule>().Register(this);
        }

        private void OnDisable()
        {
            ModuleSystem.GetModule<ICharacterModule>().Unregister(this);
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
