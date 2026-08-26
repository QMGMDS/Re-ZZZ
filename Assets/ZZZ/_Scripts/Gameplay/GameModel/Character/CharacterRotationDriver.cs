using System;

using UnityEngine;

using GamePlay.Data;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 角色旋转器
    /// </summary>
    public sealed class CharacterRotationDriver
    {
        private readonly Transform _characterTransform;

        // 本帧旋转量
        private Quaternion _targetRotation;
        // 标记 - 本帧是否有旋转量
        private bool _hasTargetRotation;

        public CharacterRotationDriver(Transform characterTransform)
        {
            _characterTransform = characterTransform;
        }

        /// <summary>
        /// 根据当前动作的旋转速度推进角色朝向
        /// </summary>
        public void Evaluate(CharacterActionAsset currentAction, Vector2 moveInput, float deltaTime)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            UpdateTargetRotation(moveInput);

            if (!_hasTargetRotation)
            {
                return;
            }

            float maxDegreesDelta = currentAction.RotationSpeedDegreesPerSecond * deltaTime;
            _characterTransform.rotation = Quaternion.RotateTowards(_characterTransform.rotation, _targetRotation, maxDegreesDelta);
        }

        private void UpdateTargetRotation(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude == 0f)
            {
                return;
            }

            Vector3 worldMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            _targetRotation = Quaternion.LookRotation(worldMoveDirection, Vector3.up);
            _hasTargetRotation = true;
        }
    }
}
