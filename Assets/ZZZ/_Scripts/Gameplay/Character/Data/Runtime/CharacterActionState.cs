using System;

using UnityEngine;

namespace GamePlay.Character
{
    [Serializable]
    public struct CharacterActionState
    {
        public Vector2 MoveDirectionInWorld;
        public Vector2 ActionDirectionInWorld;
        public CharacterIntention Intention;
        public CharacterFact Fact;
        public string CurrentActionId;
        public float LogicalProgressSeconds;

        public CharacterActionState(
            Vector2 moveDirectionInWorld,
            Vector2 actionDirectionInWorld,
            CharacterIntention intention,
            CharacterFact fact,
            string currentActionId,
            float logicalProgressSeconds)
        {
            ValidateDirection(moveDirectionInWorld, nameof(moveDirectionInWorld));
            ValidateDirection(actionDirectionInWorld, nameof(actionDirectionInWorld));

            if (string.IsNullOrWhiteSpace(currentActionId))
            {
                throw new System.ArgumentException(
                    "当前动作 ID 不能为空",
                    nameof(currentActionId));
            }

            if (float.IsNaN(logicalProgressSeconds)
                || float.IsInfinity(logicalProgressSeconds)
                || logicalProgressSeconds < 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(logicalProgressSeconds));
            }

            intention.ValidateRuntime();
            fact.ValidateRuntime();

            MoveDirectionInWorld = moveDirectionInWorld;
            ActionDirectionInWorld = actionDirectionInWorld;
            Intention = intention;
            Fact = fact;
            CurrentActionId = currentActionId;
            LogicalProgressSeconds = logicalProgressSeconds;
        }

        public static CharacterActionState CreateInitial(string currentActionId)
        {
            return new CharacterActionState(
                Vector2.zero,
                Vector2.zero,
                CharacterIntention.AllFalse,
                CharacterFact.AllFalse,
                currentActionId,
                0f);
        }

        private static void ValidateDirection(Vector2 direction, string parameterName)
        {
            if (float.IsNaN(direction.x)
                || float.IsInfinity(direction.x)
                || float.IsNaN(direction.y)
                || float.IsInfinity(direction.y))
            {
                throw new System.ArgumentException(
                    "角色方向必须是有限向量",
                    parameterName);
            }
        }
    }
}
