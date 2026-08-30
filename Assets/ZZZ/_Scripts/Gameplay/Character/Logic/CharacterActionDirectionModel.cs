using System;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 处理动作方向的捕获和实时更新
    /// </summary>
    public sealed class CharacterActionDirectionModel
    {
        private Vector3 _currentWorldDirection = Vector3.forward;
        private CharacterActionDirectionMode _currentDirectionMode;
        private bool _hasCurrentWorldDirection;

        public Vector3 CurrentWorldDirection => _currentWorldDirection;
        public CharacterActionDirectionMode CurrentDirectionMode => _currentDirectionMode;
        public bool DirectionStarted { get; private set; }

        /// <summary>
        /// 重置动作方向状态
        /// </summary>
        public void Reset()
        {
            _currentWorldDirection = Vector3.forward;
            _currentDirectionMode = default;
            DirectionStarted = false;
            _hasCurrentWorldDirection = false;
        }

        /// <summary>
        /// 根据动作方向模式更新当前世界方向
        /// </summary>
        public void Evaluate(
            CharacterActionAsset currentAction,
            bool actionStarted,
            Vector2 worldMoveDirection,
            Vector3 currentFacingDirection)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            DirectionStarted = actionStarted || !_hasCurrentWorldDirection;
            if (DirectionStarted)
            {
                _currentDirectionMode = currentAction.DirectionMode;
                _currentWorldDirection = ResolveActionWorldDirection(
                    currentAction,
                    _currentDirectionMode,
                    worldMoveDirection,
                    currentFacingDirection);
                _hasCurrentWorldDirection = true;
                return;
            }

            if (_currentDirectionMode == CharacterActionDirectionMode.LiveMoveDirection)
            {
                _currentWorldDirection = ToWorldDirection(worldMoveDirection);
            }
        }

        private static Vector3 ResolveActionWorldDirection(
            CharacterActionAsset currentAction,
            CharacterActionDirectionMode directionMode,
            Vector2 worldMoveDirection,
            Vector3 currentFacingDirection)
        {
            switch (directionMode)
            {
                case CharacterActionDirectionMode.LiveMoveDirection:
                    return ToWorldDirection(worldMoveDirection);
                case CharacterActionDirectionMode.CaptureMoveDirectionOnEnter:
                    Vector3 moveDirection = ToWorldDirection(worldMoveDirection);
                    if (moveDirection.sqrMagnitude == 0f)
                    {
                        throw new InvalidOperationException(
                            $"动作 {currentAction.Id} 要求动作开始时存在移动方向");
                    }

                    return moveDirection;
                case CharacterActionDirectionMode.CaptureFacingDirectionOnEnter:
                    currentFacingDirection.y = 0f;
                    if (currentFacingDirection.sqrMagnitude == 0f)
                    {
                        throw new InvalidOperationException(
                            $"动作 {currentAction.Id} 要求角色具有有效水平朝向");
                    }

                    return currentFacingDirection.normalized;
                default:
                    throw new InvalidOperationException(
                        $"无法解析动作 {currentAction.Id} 的方向模式");
            }
        }

        private static Vector3 ToWorldDirection(Vector2 worldMoveDirection)
        {
            Vector3 worldDirection = new Vector3(
                worldMoveDirection.x,
                0f,
                worldMoveDirection.y);

            return worldDirection.sqrMagnitude == 0f
                ? Vector3.zero
                : worldDirection.normalized;
        }
    }
}
