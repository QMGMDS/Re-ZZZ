using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.Character
{
    public sealed class PlayerCharacterCoordinator : IDisposable
    {
        private readonly CharacterActionArbiter _arbiter;
        private readonly CharacterActionTransition _transition;
        private readonly CharacterAnimationPlayer _animationPlayer;
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> _linksBySourceActionId;

        private bool _hasEnteredCurrentAction;

        public PlayerCharacterCoordinator(
            CharacterActionSetAsset characterActionSetAsset,
            CharacterController characterController,
            Transform characterTransform,
            Animator animator)
        {
            characterActionSetAsset.BuildRuntimeLookups(
                out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
                out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId);
            _actionsById = actionsById;
            _linksBySourceActionId = linksBySourceActionId;
        }

        public void Reset(CharacterActionAsset initialAction, ref CharacterActionState state)
        {
        }

        public void Tick(ref CharacterActionState state, float deltaTime)
        {
        }

        public void Dispose()
        {
            _animationPlayer.Dispose();
        }
    }
}
