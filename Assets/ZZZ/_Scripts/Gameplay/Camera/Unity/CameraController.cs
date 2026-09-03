using System;

using UnityEngine;

using SPFramework;
using GamePlay.Camera.Public;
using GamePlay.Character.Public;

namespace GamePlay.Camera
{
    [DisallowMultipleComponent]
    public sealed class CameraController : MonoBehaviour, ICameraService
    {
        [Header("跟随配置")]
        [SerializeField, Tooltip("主摄像机")]
        private UnityEngine.Camera _camera;
        [SerializeField, Tooltip("需要跟随目标的指定物体")]
        private Transform _specifiedObject;
        [SerializeField, Tooltip("默认跟随的目标物体")]
        private Transform _targetObject;
        [SerializeField, Min(0.0001f), Tooltip("跟随平滑时间 单位为秒")]
        private float _smoothTimeSeconds = 0.2f;

        private ObjectFollower _objectFollower;

        // 事件退订句柄
        private IDisposable _characterSwitchedSubscription;

        private void Awake()
        {
            if (_camera == null || _specifiedObject == null || _targetObject == null || _smoothTimeSeconds <= 0f)
            {
                throw new InvalidOperationException($"{nameof(CameraController)} 检查配置");
            }

            _objectFollower = new ObjectFollower(_specifiedObject, _smoothTimeSeconds);
            _objectFollower.SetTargetObject(_targetObject);
        }

        private void OnEnable()
        {
            ServiceHub.Register<ICameraService>(this);
            _characterSwitchedSubscription = EventBus.Subscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
        }

        private void OnDisable()
        {
            ServiceHub.Unregister<ICameraService>(this);
            _characterSwitchedSubscription.Dispose();
        }

        public void CameraUpdate()
        {
            _objectFollower.Follow();
        }

        #region 服务接口

        /// <inheritdoc/>
        public Vector2 ConvertToWorldCoordinate(Vector2 input)
        {
            Vector3 cameraForward = _camera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = _camera.transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 worldDirection = cameraRight * input.x + cameraForward * input.y;
            return new Vector2(worldDirection.x, worldDirection.z);
        }

        #endregion

        #region 事件回调

        private void OnCharacterSwitched(CharacterSwitchedEvent characterSwitchedEvent)
        {
            _objectFollower.SetTargetObject(characterSwitchedEvent.CharacterTransform);
        }

        #endregion
    }
}
