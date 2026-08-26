using System;

using UnityEngine;

using GamePlay.Contract;
using GamePlay.Data;
using GamePlay.GameModule;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 角色动作控制 Root Mono
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterActionController : MonoBehaviour, IInputCharacter
    {
        [Header("必要组件")]
        [SerializeField, Tooltip("负责角色碰撞移动的 CharacterController")] private CharacterController _characterController;
        [SerializeField, Tooltip("显示本角色动画的 Animator")] private Animator _animator;

        [Header("自定义配置")]
        [SerializeField, Tooltip("本角色的动作资产集合")] private CharacterActionSetAsset _actionSet;

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
        // 最近一次输入给角色的数据
        private InputCharacterData _inputCharacterData;
        // 角色所处事实
        private CharacterFact _fact;

        private void Awake()
        {
            if (_characterController == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterActionController)} 要求必须分配 {nameof(_characterController)}");
            }

            if (_animator == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterActionController)} 要求必须分配 {nameof(_animator)}");
            }

            if (_actionSet == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterActionController)} 要求必须分配 {nameof(_actionSet)}");
            }

            _actionSet.BuildRuntimeLookups(out var actionsById, out var linksBySourceActionId);

            _arbiter = new CharacterActionArbiter(actionsById, linksBySourceActionId);
            _transition = new CharacterActionTransition();

            _displacementDriver = new CharacterDisplacementDriver(_characterController);
            _rotationDriver = new CharacterRotationDriver(transform);
            _animationDriver = new CharacterAnimationDriver(_animator, linksBySourceActionId);

            // 第一个动作为默认动作
            _currentAction = _actionSet.Actions[0];

            _fact = new CharacterFact(Trilean.False, Trilean.False);
        }

        private void Update()
        {
            // 裁决
            CharacterActionAsset targetAction = _arbiter.TrySwitch(_currentAction.Id, _inputCharacterData.Intention, _fact);
            // 过渡
            float logicalProgressSeconds = _transition.Tick(targetAction, ref _currentAction, Time.deltaTime, ref _fact);

            // 位移
            _displacementDriver.Evaluate(_currentAction, logicalProgressSeconds, _inputCharacterData.MoveInput);
            // 旋转
            _rotationDriver.Evaluate(_currentAction, _inputCharacterData.MoveInput, Time.deltaTime);
            // 动画表现
            _animationDriver.Evaluate(_currentAction, logicalProgressSeconds, Time.deltaTime);
        }

        /// <inheritdoc/>
        public void InputCharacter(InputCharacterData inputCharacterData)
        {
            _inputCharacterData = inputCharacterData;
        }

        private void OnDestroy()
        {
            _animationDriver.Dispose();
        }
    }
}
