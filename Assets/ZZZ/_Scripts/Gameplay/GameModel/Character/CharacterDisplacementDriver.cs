using System;

using UnityEngine;

using GamePlay.Data;

namespace GamePlay.GameModel
{
    /// <summary>
    /// 角色位移器
    /// </summary>
    public sealed class CharacterDisplacementDriver
    {
        private readonly CharacterController _characterController;

        private CharacterActionAsset _currentAction;
        private float _previousLogicalProgressSeconds;
        // 最近一次有效的世界空间移动方向
        private Vector3 _lastWorldMoveDirection = Vector3.forward;

        public CharacterDisplacementDriver(CharacterController characterController)
        {
            _characterController = characterController;
        }

        /// <summary>
        /// 推进当前动作位移并写入 CharacterController
        /// </summary>
        public void Evaluate(
            CharacterActionAsset currentAction,
            float logicalProgressSeconds,
            Vector2 worldMoveDirection)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            if (logicalProgressSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalProgressSeconds));
            }

            ResetProgressIfActionRestarted(currentAction, logicalProgressSeconds);
            UpdateLastWorldMoveDirection(worldMoveDirection);

            float previousZ = currentAction.EvaluateZDisplacement(_previousLogicalProgressSeconds);
            float currentZ = currentAction.EvaluateZDisplacement(logicalProgressSeconds);
            float deltaZ = currentZ - previousZ;

            if (deltaZ != 0f)
            {
                _characterController.Move(_lastWorldMoveDirection * deltaZ);
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

        private void UpdateLastWorldMoveDirection(Vector2 worldMoveDirection)
        {
            if (worldMoveDirection.sqrMagnitude == 0f)
            {
                return;
            }

            _lastWorldMoveDirection = new Vector3(
                worldMoveDirection.x,
                0f,
                worldMoveDirection.y).normalized;
        }
    }
}
