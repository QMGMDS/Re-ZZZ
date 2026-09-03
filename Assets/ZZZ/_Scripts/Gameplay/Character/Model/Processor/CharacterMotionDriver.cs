using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 驱动角色的旋转和位移
    /// </summary>
    public sealed class CharacterMotionDriver
    {
        private readonly CharacterController _characterController;
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;

        private string _currentActionId;
        private Vector3 _capturedDirectionInWorld;
        private float _previousNormalizedProgress;
        private bool _hasCurrentAction;

        public CharacterMotionDriver(CharacterController characterController, IReadOnlyDictionary<string, CharacterActionAsset> actionsById)
        {
            _characterController = characterController;
            _actionsById = actionsById;
        }

        public void MotionUpdate(ref CharacterActionState state, bool hasActionTransition)
        {
            CharacterActionAsset currentAction = _actionsById[state.CurrentActionId];
            bool isActionEnter = !_hasCurrentAction
                || hasActionTransition
                || !string.Equals(_currentActionId, state.CurrentActionId, StringComparison.Ordinal);

            if (isActionEnter)
            {
                _currentActionId = state.CurrentActionId;
                _previousNormalizedProgress = 0f;
                _capturedDirectionInWorld = CaptureDirectionOnEnter(currentAction.ActionDirectionMode, state);
                _hasCurrentAction = true;
            }

            float normalizedProgress = GetNormalizedProgress(
                state.LogicalProgressSeconds,
                currentAction.DurationSeconds,
                currentAction.ActionId);
            Vector3 directionInWorld = ResolveDirection(currentAction.ActionDirectionMode, state);

            ApplyRotation(
                currentAction.ActionDirectionMode,
                directionInWorld,
                currentAction.MaxRotationSpeedDegreesPerSecond);

            float currentDisplacement = EvaluateCumulativeDisplacement(currentAction, normalizedProgress);
            float previousDisplacement = EvaluateCumulativeDisplacement(currentAction, _previousNormalizedProgress);
            float displacementDelta = currentDisplacement - previousDisplacement;

            if (directionInWorld.sqrMagnitude != 0f && displacementDelta != 0f)
            {
                _characterController.Move(directionInWorld * displacementDelta);
            }

            _previousNormalizedProgress = normalizedProgress;
        }

        private Vector3 CaptureDirectionOnEnter(ActionDirectionMode actionDirectionMode, CharacterActionState state)
        {
            switch (actionDirectionMode)
            {
                case ActionDirectionMode.LiveMoveDirection:
                    return Vector3.zero;
                case ActionDirectionMode.CaptureMoveDirectionOnEnter:
                    return ToHorizontalDirection(state.MoveDirectionInWorld);
                case ActionDirectionMode.CaptureFacingDirectionOnEnter:
                    return ToHorizontalDirection(_characterController.transform.forward);
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionDirectionMode), actionDirectionMode, "不支持的动作运动模式");
            }
        }

        private Vector3 ResolveDirection(ActionDirectionMode actionDirectionMode, CharacterActionState state)
        {
            switch (actionDirectionMode)
            {
                case ActionDirectionMode.LiveMoveDirection:
                    return ToHorizontalDirection(state.MoveDirectionInWorld);
                case ActionDirectionMode.CaptureMoveDirectionOnEnter:
                case ActionDirectionMode.CaptureFacingDirectionOnEnter:
                    return _capturedDirectionInWorld;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionDirectionMode), actionDirectionMode, "不支持的动作运动模式");
            }
        }

        private void ApplyRotation(ActionDirectionMode actionDirectionMode, Vector3 directionInWorld, float maxRotationSpeedDegreesPerSecond)
        {
            if (actionDirectionMode == ActionDirectionMode.CaptureMoveDirectionOnEnter)
            {
                ApplyFacingDirection(directionInWorld);
                return;
            }

            RotateTowards(directionInWorld, maxRotationSpeedDegreesPerSecond);
        }

        private void ApplyFacingDirection(Vector3 directionInWorld)
        {
            if (directionInWorld.sqrMagnitude == 0f)
            {
                return;
            }

            Vector3 eulerAngles = _characterController.transform.eulerAngles;
            eulerAngles.y = Quaternion.LookRotation(directionInWorld, Vector3.up).eulerAngles.y;
            _characterController.transform.eulerAngles = eulerAngles;
        }

        private void RotateTowards(Vector3 directionInWorld, float maxRotationSpeedDegreesPerSecond)
        {
            if (directionInWorld.sqrMagnitude == 0f || maxRotationSpeedDegreesPerSecond == 0f)
            {
                return;
            }

            float targetYaw = Quaternion.LookRotation(directionInWorld, Vector3.up).eulerAngles.y;
            Vector3 eulerAngles = _characterController.transform.eulerAngles;
            eulerAngles.y = Mathf.MoveTowardsAngle(
                eulerAngles.y,
                targetYaw,
                maxRotationSpeedDegreesPerSecond * Time.deltaTime);
            _characterController.transform.eulerAngles = eulerAngles;
        }

        private static float GetNormalizedProgress(float logicalProgressSeconds, float durationSeconds, string actionId)
        {
            if (durationSeconds <= 0f)
            {
                throw new InvalidOperationException(
                    $"动作 {actionId} 的逻辑时长必须大于 0");
            }

            return Mathf.Clamp01(logicalProgressSeconds / durationSeconds);
        }

        private static float EvaluateCumulativeDisplacement(CharacterActionAsset action, float normalizedProgress)
        {
            AnimationCurve displacementCurve = action.CumulativeForwardDisplacement;
            if (displacementCurve == null || displacementCurve.length == 0)
            {
                return 0f;
            }

            return displacementCurve.Evaluate(normalizedProgress);
        }

        private static Vector3 ToHorizontalDirection(Vector2 directionInWorld)
        {
            return ToHorizontalDirection(new Vector3(directionInWorld.x, 0f, directionInWorld.y));
        }

        private static Vector3 ToHorizontalDirection(Vector3 directionInWorld)
        {
            directionInWorld.y = 0f;
            if (directionInWorld.sqrMagnitude == 0f)
            {
                return Vector3.zero;
            }

            return directionInWorld.normalized;
        }
    }
}
