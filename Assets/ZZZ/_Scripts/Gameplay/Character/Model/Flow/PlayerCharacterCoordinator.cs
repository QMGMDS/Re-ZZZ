using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.Character
{
    public sealed class PlayerCharacterCoordinator : IDisposable
    {
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> _linksBySourceActionId;

        private readonly CharacterActionArbiter _arbiter;
        private readonly CharacterActionTransition _transition;
        private readonly CharacterMotionDriver _motionDriver;
        private readonly CharacterAnimationPlayer _animationPlayer;

        // 当前动作状态
        private CharacterActionState _currentActionState;

        public PlayerCharacterCoordinator(CharacterActionSetAsset characterActionSetAsset, Animator animator, CharacterController characterController)
        {
            characterActionSetAsset.BuildRuntimeLookups(
                out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
                out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId);
            _actionsById = actionsById;
            _linksBySourceActionId = linksBySourceActionId;

            _arbiter = new CharacterActionArbiter(_linksBySourceActionId, _actionsById);
            _transition = new CharacterActionTransition();
            _motionDriver = new CharacterMotionDriver(characterController, _actionsById);
            _animationPlayer = new CharacterAnimationPlayer(_actionsById, animator);

            _currentActionState = new CharacterActionState(characterActionSetAsset.InitialAction.ActionId);
        }

        public void Tick()
        {
            // 动作切换 actionLink为命中有向边
            bool hasActionTransition = _arbiter.Arbitrate(ref _currentActionState, out CharacterActionLink actionLink);
            // 动作过渡
            _transition.Advance(ref _currentActionState);

            // 角色运动响应
            _motionDriver.MotionUpdate(ref _currentActionState, hasActionTransition);

            // 动画响应
            _animationPlayer.AnimationPlay(ref _currentActionState, hasActionTransition ? actionLink.AnimationBlendSeconds : 0f);
        }

        public void Dispose()
        {
            _animationPlayer.Dispose();
        }

        public void SetIntention(CharacterIntention intention, Vector2 moveDirectionInWorld)
        {
            _currentActionState.SetIntention(intention);
            _currentActionState.SetMoveDirectionInWorld(moveDirectionInWorld);
        }

        public void MarkEnterField()
        {
            _currentActionState.SetFact(_currentActionState.Fact.MarkEnterField());
        }

        public void MarkExitField()
        {
            _currentActionState.SetFact(_currentActionState.Fact.MarkExitField());
        }
    }
}
