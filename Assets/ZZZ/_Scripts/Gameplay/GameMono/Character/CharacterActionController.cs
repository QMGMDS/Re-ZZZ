using System;

using UnityEngine;

using GamePlay.Contract;
using GamePlay.Data;
using GamePlay.GameModule;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 连接角色动作裁决器与过渡器的 Mono 层入口
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterActionController : MonoBehaviour, IInputCharacter
    {
        [SerializeField, Tooltip("本角色的动作资产集合")] private CharacterActionSetAsset _actionSet;
        [SerializeField, Tooltip("显示本角色动画的 Animator")] private Animator _animator;

        // 仲裁器
        private CharacterActionArbiter _arbiter;
        // 过渡器
        private CharacterActionTransition _transition;
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
            if (_actionSet == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterActionController)} 要求必须分配 {nameof(_actionSet)}");
            }

            if (_animator == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterActionController)} 要求必须分配 {nameof(_animator)}");
            }

            _actionSet.BuildRuntimeLookups(out var actionsById, out var linksBySourceActionId);
            _arbiter = new CharacterActionArbiter(actionsById, linksBySourceActionId);
            _transition = new CharacterActionTransition();
            _animationDriver = new CharacterAnimationDriver(_animator);
            // 第一个动作为默认动作
            _currentAction = _actionSet.Actions[0];

            _fact = new CharacterFact(Trilean.False, Trilean.False);
        }

        private void Update()
        {
            CharacterActionAsset targetAction = _arbiter.TrySwitch(_currentAction.Id, _inputCharacterData.Intention, _fact);
            float logicalProgressSeconds = _transition.Tick(targetAction, ref _currentAction, Time.deltaTime, ref _fact);
            _animationDriver.Evaluate(_currentAction, logicalProgressSeconds);
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
