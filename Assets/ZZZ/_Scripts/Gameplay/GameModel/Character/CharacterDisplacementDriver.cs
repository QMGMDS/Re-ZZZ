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
        // 本帧移动朝向 1朝前 -1朝后
        private int _movementDirection = 1;

        public CharacterDisplacementDriver(CharacterController characterController)
        {
            _characterController = characterController;
        }

        /// <summary>
        /// 推进当前动作位移并写入 CharacterController
        /// </summary>
        public void Evaluate(CharacterActionAsset currentAction, float logicalProgressSeconds, Vector2 moveInput)
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
            UpdateMovementDirection(moveInput);

            float previousZ = currentAction.EvaluateZDisplacement(_previousLogicalProgressSeconds);
            float currentZ = currentAction.EvaluateZDisplacement(logicalProgressSeconds);
            float deltaZ = currentZ - previousZ;

            if (deltaZ != 0f)
            {
                Vector3 forward = _characterController.transform.forward;
                forward.y = 0f;
                forward.Normalize();

                _characterController.Move(forward * (deltaZ * _movementDirection));
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

        // 这里的 moveInput 是世界空间中角色想要移动的方向
        private void UpdateMovementDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude == 0f)
            {
                return;
            }

            Vector3 worldMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 forward = _characterController.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            _movementDirection = Vector3.Dot(forward, worldMoveDirection) < 0f ? -1 : 1;
        }
    }
}
