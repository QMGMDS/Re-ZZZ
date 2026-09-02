using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.Character
{
    public sealed class CharacterActionTransition
    {
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;

        public CharacterActionTransition(CharacterActionSetAsset actionSet)
        {
            if (actionSet == null)
            {
                throw new ArgumentNullException(nameof(actionSet));
            }

            actionSet.BuildRuntimeLookups(
                out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
                out _);
            _actionsById = actionsById;
        }

        public CharacterActionTransition(IReadOnlyDictionary<string, CharacterActionAsset> actionsById)
        {
            _actionsById = actionsById ?? throw new ArgumentNullException(nameof(actionsById));
        }

        public CharacterActionAsset GetAction(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                throw new ArgumentException("动作 ID 不能为空", nameof(actionId));
            }

            if (!_actionsById.TryGetValue(actionId, out CharacterActionAsset action))
            {
                throw new InvalidOperationException($"动作不存在 {actionId}");
            }

            return action;
        }

        public CharacterActionAsset ApplySelectedLink(
            ref CharacterActionState state,
            in CharacterActionLink selectedLink)
        {
            if (string.IsNullOrWhiteSpace(state.CurrentActionId))
            {
                throw new InvalidOperationException("当前动作 ID 不能为空");
            }

            if (!string.Equals(state.CurrentActionId, selectedLink.SourceActionId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"动作链接源 ID 与当前动作不一致 {selectedLink.SourceActionId}");
            }

            GetAction(state.CurrentActionId);
            selectedLink.RequiredIntention.ValidateCondition();
            CharacterActionAsset targetAction = GetAction(selectedLink.TargetActionId);
            state.Intention.ValidateRuntime();
            state.Fact = state.Fact.ConsumeRequired(selectedLink.RequiredFact);
            state.CurrentActionId = targetAction.ActionId;
            state.LogicalProgressSeconds = 0f;
            return targetAction;
        }

        public void Advance(ref CharacterActionState state, float deltaTime)
        {
            if (float.IsNaN(deltaTime)
                || float.IsInfinity(deltaTime)
                || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            CharacterActionAsset currentAction = GetAction(state.CurrentActionId);
            state.Fact.ValidateRuntime();
            state.Intention.ValidateRuntime();

            if (float.IsNaN(state.LogicalProgressSeconds)
                || float.IsInfinity(state.LogicalProgressSeconds)
                || state.LogicalProgressSeconds < 0f)
            {
                throw new InvalidOperationException("当前动作逻辑进度无效");
            }

            state.LogicalProgressSeconds = Mathf.Min(
                state.LogicalProgressSeconds + deltaTime,
                currentAction.DurationSeconds);
        }
    }
}
