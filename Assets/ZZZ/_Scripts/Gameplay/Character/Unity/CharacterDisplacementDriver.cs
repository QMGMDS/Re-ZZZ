using System;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色位移器
    /// </summary>
    public sealed class CharacterDisplacementDriver
    {
        private readonly CharacterController _characterController;

        private CharacterActionAsset _currentAction;
        private float _previousLogicalProgressSeconds;

        public CharacterDisplacementDriver(CharacterController characterController)
        {
            _characterController = characterController;
        }

        /// <summary>
        /// 推进当前动作位移并写入 CharacterController
        /// </summary>
        public void Evaluate(CharacterActionState actionState)
        {
            CharacterActionAsset currentAction = actionState.CurrentAction;
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            float logicalProgressSeconds = actionState.LogicalProgressSeconds;
            if (logicalProgressSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalProgressSeconds));
            }

            Vector3 worldDisplacementDirection = actionState.WorldDirection;

            ResetProgressIfActionRestarted(currentAction, logicalProgressSeconds);

            float previousZ = currentAction.EvaluateZDisplacement(_previousLogicalProgressSeconds);
            float currentZ = currentAction.EvaluateZDisplacement(logicalProgressSeconds);
            float deltaZ = currentZ - previousZ;

            if (deltaZ != 0f)
            {
                _characterController.Move(worldDisplacementDirection * deltaZ);
            }

            _previousLogicalProgressSeconds = logicalProgressSeconds;
        }

        private void ResetProgressIfActionRestarted(CharacterActionAsset currentAction, float logicalProgressSeconds)
        {
            if (_currentAction == currentAction && logicalProgressSeconds >= _previousLogicalProgressSeconds)
            {
                return;
            }

            _currentAction = currentAction;
            _previousLogicalProgressSeconds = 0f;
        }
    }
}
