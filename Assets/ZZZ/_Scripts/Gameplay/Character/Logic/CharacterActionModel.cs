using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色动作逻辑模型
    /// </summary>
    public sealed class CharacterActionModel
    {
        private readonly CharacterInfoRuntime _runtime;
        private readonly CharacterActionArbiter _arbiter;
        private readonly CharacterActionTransition _transition;
        private readonly CharacterActionDirectionModel _directionModel;

        private CharacterActionState _currentState;

        public CharacterInfoRuntime Runtime => _runtime;
        public CharacterActionState CurrentState => _currentState;

        public CharacterActionModel(
            CharacterInfoAsset characterInfoAsset,
            CharacterActionAsset defaultAction,
            IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
            IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            if (characterInfoAsset == null || defaultAction == null)
            {
                throw new ArgumentNullException(
                    characterInfoAsset == null
                        ? nameof(characterInfoAsset)
                        : nameof(defaultAction));
            }

            if (actionsById == null || linksBySourceActionId == null)
            {
                throw new ArgumentNullException(
                    actionsById == null
                        ? nameof(actionsById)
                        : nameof(linksBySourceActionId));
            }

            _runtime = new CharacterInfoRuntime(characterInfoAsset);
            _arbiter = new CharacterActionArbiter(actionsById, linksBySourceActionId);
            _transition = new CharacterActionTransition(defaultAction);
            _directionModel = new CharacterActionDirectionModel();
            _currentState = new CharacterActionState(
                defaultAction,
                0f,
                Vector3.zero,
                defaultAction.DirectionMode,
                false,
                false);
        }

        /// <summary>
        /// 写入角色本次逻辑 Tick 的输入数据
        /// </summary>
        public void WriteRuntimeData(InputCharacterData inputCharacterData)
        {
            _runtime.Intention = inputCharacterData.Intention;
            _runtime.MoveDirection = inputCharacterData.MoveInput;
        }

        /// <summary>
        /// 写入一次角色受击事实
        /// </summary>
        public void ReceiveHit(int damage)
        {
            _runtime.Fact = _runtime.Fact.MarkHit();
        }

        /// <summary>
        /// 推进一次角色动作逻辑 Tick
        /// </summary>
        public CharacterActionState LogicUpdate(
            float tickDeltaSeconds,
            Vector3 currentFacingDirection)
        {
            CharacterFact fact = _runtime.Fact;
            float currentActionProgress = _transition.GetNormalizedProgress();
            bool hitReceived = fact.Hit == Trilean.True;

            CharacterActionAsset targetAction = _arbiter.TrySwitch(
                _transition.CurrentAction.Id,
                currentActionProgress,
                _runtime.Intention,
                fact);

            if (hitReceived)
            {
                _runtime.Fact = fact.ConsumeHit();
            }

            bool restartCurrentAction =
                hitReceived
                && targetAction == _transition.CurrentAction;

            bool actionStarted;
            float logicalProgressSeconds = _transition.Tick(
                targetAction,
                tickDeltaSeconds,
                restartCurrentAction,
                out actionStarted);

            _directionModel.Evaluate(
                _transition.CurrentAction,
                actionStarted,
                _runtime.MoveDirection,
                currentFacingDirection);

            _currentState = new CharacterActionState(
                _transition.CurrentAction,
                logicalProgressSeconds,
                _directionModel.CurrentWorldDirection,
                _directionModel.CurrentDirectionMode,
                actionStarted,
                _directionModel.DirectionStarted);

            return _currentState;
        }
    }
}
