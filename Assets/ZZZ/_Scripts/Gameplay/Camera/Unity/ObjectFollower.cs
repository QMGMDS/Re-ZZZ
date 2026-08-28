using System;

using UnityEngine;

namespace GamePlay.Camera
{
    /// <summary>
    /// 物体跟随器
    /// </summary>
    public sealed class ObjectFollower
    {
        private readonly Transform _specifiedObject;
        private readonly float _smoothTimeSeconds;

        private Transform _targetObject;
        private Vector2 _smoothDampVelocity;

        public ObjectFollower(Transform specifiedObject, float smoothTimeSeconds)
        {
            if (specifiedObject == null)
            {
                throw new ArgumentNullException(nameof(specifiedObject));
            }

            if (smoothTimeSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(smoothTimeSeconds));
            }

            _specifiedObject = specifiedObject;
            _smoothTimeSeconds = smoothTimeSeconds;
        }

        /// <summary>
        /// 设置跟随目标物体
        /// </summary>
        public void SetTargetObject(Transform targetObject)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            _targetObject = targetObject;
        }

        /// <summary>
        /// 驱动指定物体跟随目标物体
        /// </summary>
        public void Follow(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            Vector3 specifiedPosition = _specifiedObject.position;
            Vector3 targetPosition = _targetObject.position;

            Vector2 currentPosition = new Vector2(specifiedPosition.x, specifiedPosition.z);
            Vector2 targetPositionXZ = new Vector2(targetPosition.x, targetPosition.z);
            Vector2 nextPosition = Vector2.SmoothDamp(
                currentPosition,
                targetPositionXZ,
                ref _smoothDampVelocity,
                _smoothTimeSeconds,
                Mathf.Infinity,
                deltaTime);

            specifiedPosition.x = nextPosition.x;
            specifiedPosition.z = nextPosition.y;
            _specifiedObject.position = specifiedPosition;
        }
    }
}
