using System;

using UnityEngine;

namespace GamePlay.Character
{
    public sealed class CharacterRotationDriver
    {
        private const float MIN_HORIZONTAL_DIRECTION_SQR_MAGNITUDE = 0.00000001f;

        private readonly Transform _characterTransform;

        public CharacterRotationDriver(Transform characterTransform)
        {
            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            _characterTransform = characterTransform;
        }

        public void Evaluate(
            ref CharacterActionState state,
            CharacterActionAsset currentAction,
            bool actionEntered,
            float deltaTime)
        {
            UpdateActionDirection(ref state, currentAction, actionEntered);
            ApplyRotation(state, currentAction, actionEntered, deltaTime);
        }

        public void UpdateActionDirection(
            ref CharacterActionState state,
            CharacterActionAsset currentAction,
            bool actionEntered)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            ValidateMoveDirection(state.MoveDirectionInWorld);

            if (actionEntered)
            {
                Vector3 enteredDirection = ResolveEnteredDirection(
                    currentAction,
                    state.MoveDirectionInWorld);
                state.ActionDirectionInWorld = ToVector2(enteredDirection);

                if (currentAction.ActionDirectionMode != ActionDirectionMode.LiveMoveDirection)
                {
                    return;
                }
            }
            else if (currentAction.ActionDirectionMode == ActionDirectionMode.LiveMoveDirection)
            {
                state.ActionDirectionInWorld = ToVector2(
                    ToHorizontalDirection(state.MoveDirectionInWorld));
            }
            else
            {
                return;
            }
        }

        public void ApplyRotation(
            in CharacterActionState state,
            CharacterActionAsset currentAction,
            bool actionEntered,
            float deltaTime)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            ValidateMoveDirection(state.ActionDirectionInWorld);

            if (currentAction.ActionDirectionMode != ActionDirectionMode.LiveMoveDirection)
            {
                if (actionEntered)
                {
                    AlignImmediately(ToWorldDirection(state.ActionDirectionInWorld));
                }

                return;
            }

            if (state.ActionDirectionInWorld == Vector2.zero)
            {
                return;
            }

            Vector3 targetDirection = ToWorldDirection(state.ActionDirectionInWorld);
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            float maxDegreesDelta = currentAction.MaxRotationSpeedDegreesPerSecond * deltaTime;
            _characterTransform.rotation = Quaternion.RotateTowards(
                _characterTransform.rotation,
                targetRotation,
                maxDegreesDelta);
        }

        private Vector3 ResolveEnteredDirection(CharacterActionAsset action, Vector2 moveDirection)
        {
            switch (action.ActionDirectionMode)
            {
                case ActionDirectionMode.LiveMoveDirection:
                    return ToHorizontalDirection(moveDirection);
                case ActionDirectionMode.CaptureMoveDirectionOnEnter:
                    Vector3 moveWorldDirection = ToHorizontalDirection(moveDirection);
                    if (moveWorldDirection == Vector3.zero)
                    {
                        throw new InvalidOperationException(
                            $"动作 {action.ActionId} 进入时需要有效移动方向");
                    }

                    return moveWorldDirection;
                case ActionDirectionMode.CaptureFacingDirectionOnEnter:
                    Vector3 facingDirection = _characterTransform.forward;
                    facingDirection.y = 0f;
                    if (facingDirection.sqrMagnitude <= MIN_HORIZONTAL_DIRECTION_SQR_MAGNITUDE)
                    {
                        throw new InvalidOperationException(
                            $"动作 {action.ActionId} 进入时需要有效水平朝向");
                    }

                    return facingDirection.normalized;
                default:
                    throw new InvalidOperationException(
                        $"动作 {action.ActionId} 的方向模式无效");
            }
        }

        private void AlignImmediately(Vector3 direction)
        {
            _characterTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private static Vector3 ToHorizontalDirection(Vector2 direction)
        {
            Vector3 worldDirection = ToWorldDirection(direction);
            return worldDirection.sqrMagnitude <= MIN_HORIZONTAL_DIRECTION_SQR_MAGNITUDE
                ? Vector3.zero
                : worldDirection.normalized;
        }

        private static Vector3 ToWorldDirection(Vector2 direction)
        {
            return new Vector3(direction.x, 0f, direction.y);
        }

        private static Vector2 ToVector2(Vector3 direction)
        {
            return new Vector2(direction.x, direction.z);
        }

        private static void ValidateMoveDirection(Vector2 direction)
        {
            if (float.IsNaN(direction.x)
                || float.IsInfinity(direction.x)
                || float.IsNaN(direction.y)
                || float.IsInfinity(direction.y))
            {
                throw new ArgumentException("移动方向必须是有限向量", nameof(direction));
            }
        }
    }
}
