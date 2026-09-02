using System;

using UnityEngine;

namespace GamePlay.Character
{
    public sealed class CharacterPositionDriver
    {
        private readonly CharacterController _characterController;

        private CharacterActionAsset _previousAction;
        private float _previousLogicalProgressSeconds;

        public CharacterPositionDriver(CharacterController characterController)
        {
            if (characterController == null)
            {
                throw new ArgumentNullException(nameof(characterController));
            }

            _characterController = characterController;
        }

        public void Reset()
        {
            _previousAction = null;
            _previousLogicalProgressSeconds = 0f;
        }

        public void Evaluate(
            in CharacterActionState state,
            CharacterActionAsset currentAction,
            bool actionEntered)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            ValidateProgress(state.LogicalProgressSeconds, nameof(state.LogicalProgressSeconds));
            if (state.LogicalProgressSeconds > currentAction.DurationSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(state.LogicalProgressSeconds));
            }

            ValidateDirection(state.ActionDirectionInWorld);

            if (actionEntered || !ReferenceEquals(_previousAction, currentAction))
            {
                _previousAction = currentAction;
                _previousLogicalProgressSeconds = 0f;
            }

            Vector3 displacement = CalculateDisplacement(
                currentAction,
                _previousLogicalProgressSeconds,
                state.LogicalProgressSeconds,
                state.ActionDirectionInWorld);

            if (displacement != Vector3.zero)
            {
                _characterController.Move(displacement);
            }

            _previousLogicalProgressSeconds = state.LogicalProgressSeconds;
        }

        public static Vector3 CalculateDisplacement(
            CharacterActionAsset action,
            float previousLogicalProgressSeconds,
            float currentLogicalProgressSeconds,
            Vector2 actionDirectionInWorld)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            ValidateProgress(previousLogicalProgressSeconds, nameof(previousLogicalProgressSeconds));
            ValidateProgress(currentLogicalProgressSeconds, nameof(currentLogicalProgressSeconds));
            if (previousLogicalProgressSeconds > action.DurationSeconds
                || currentLogicalProgressSeconds > action.DurationSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(currentLogicalProgressSeconds));
            }

            ValidateDirection(actionDirectionInWorld);
            if (currentLogicalProgressSeconds < previousLogicalProgressSeconds)
            {
                throw new ArgumentException(
                    "当前动作逻辑进度不能早于上一动作逻辑进度",
                    nameof(currentLogicalProgressSeconds));
            }

            float previousDisplacement =
                action.EvaluateCumulativeForwardDisplacement(previousLogicalProgressSeconds);
            float currentDisplacement =
                action.EvaluateCumulativeForwardDisplacement(currentLogicalProgressSeconds);
            float displacementScalar = currentDisplacement - previousDisplacement;

            return new Vector3(
                actionDirectionInWorld.x * displacementScalar,
                0f,
                actionDirectionInWorld.y * displacementScalar);
        }

        private static void ValidateProgress(float progress, string parameterName)
        {
            if (float.IsNaN(progress) || float.IsInfinity(progress) || progress < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateDirection(Vector2 direction)
        {
            if (float.IsNaN(direction.x)
                || float.IsInfinity(direction.x)
                || float.IsNaN(direction.y)
                || float.IsInfinity(direction.y))
            {
                throw new ArgumentException("动作方向必须是有限向量", nameof(direction));
            }
        }
    }
}
