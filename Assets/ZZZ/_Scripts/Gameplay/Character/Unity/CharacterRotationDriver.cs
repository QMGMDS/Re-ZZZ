using System;

using UnityEngine;

namespace GamePlay.Character
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
        public void Evaluate(CharacterActionState actionState, float deltaTime)
        {
            CharacterActionAsset currentAction = actionState.CurrentAction;
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (actionState.DirectionStarted)
            {
                _hasTargetRotation = false;

                if (actionState.WorldDirection.sqrMagnitude != 0f)
                {
                    _targetRotation = Quaternion.LookRotation(actionState.WorldDirection, Vector3.up);
                    _hasTargetRotation = true;

                    if (actionState.DirectionMode != CharacterActionDirectionMode.LiveMoveDirection)
                    {
                        _characterTransform.rotation = _targetRotation;
                    }
                }
            }

            if (actionState.DirectionMode == CharacterActionDirectionMode.LiveMoveDirection)
            {
                UpdateTargetRotation(actionState.WorldDirection);
            }

            if (!_hasTargetRotation)
            {
                return;
            }

            float maxDegreesDelta = currentAction.RotationSpeedDegreesPerSecond * deltaTime;
            _characterTransform.rotation = Quaternion.RotateTowards(_characterTransform.rotation, _targetRotation, maxDegreesDelta);
        }

        private void UpdateTargetRotation(Vector3 worldMoveDirection)
        {
            if (worldMoveDirection.sqrMagnitude == 0f)
            {
                _hasTargetRotation = false;
                return;
            }

            _targetRotation = Quaternion.LookRotation(worldMoveDirection, Vector3.up);
            _hasTargetRotation = true;
        }
    }
}
