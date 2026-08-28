using System;

using UnityEngine;

using GamePlay.Camera.Contract;
using SPFramework;

namespace GamePlay.Camera
{
    /// <summary>
    /// 摄像机控制器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraController : MonoBehaviour, ICameraService, ICameraUpdateTarget
    {
        [Header("跟随配置")]
        [SerializeField, Tooltip("需要跟随目标的指定物体")]
        private Transform _specifiedObject;
        [SerializeField, Tooltip("默认跟随的目标物体")]
        private Transform _targetObject;
        [SerializeField, Tooltip("主摄像机")]
        private UnityEngine.Camera _camera;
        [SerializeField, Min(0.0001f), Tooltip("跟随平滑时间 单位为秒")]
        private float _smoothTimeSeconds = 0.2f;

        private ObjectFollower _objectFollower;
        private ICameraModule _cameraModule;

        private void Awake()
        {
            if (_specifiedObject == null
                || _targetObject == null
                || _camera == null
                || _smoothTimeSeconds <= 0f)
            {
                throw new InvalidOperationException(
                    $"{nameof(CameraController)} 检查配置");
            }

            _objectFollower = new ObjectFollower(_specifiedObject, _smoothTimeSeconds);
            SetTargetObject(_targetObject);
        }

        private void OnEnable()
        {
            ServiceHub.Register<ICameraService>(this);
            ICameraModule cameraModule = ModuleSystem.GetModule<ICameraModule>();
            cameraModule.Register(this);
            _cameraModule = cameraModule;
        }

        /// <inheritdoc/>
        public void RenderUpdate(float deltaTimeSeconds)
        {
            _objectFollower.Follow(deltaTimeSeconds);
        }

        private void OnDisable()
        {
            _cameraModule.Unregister(this);
            _cameraModule = null;
            ServiceHub.Unregister<ICameraService>(this);
        }

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

        /// <inheritdoc/>
        public void SetTargetObject(Transform targetObject)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            _targetObject = targetObject;
            _objectFollower.SetTargetObject(targetObject);
        }
    }
}
