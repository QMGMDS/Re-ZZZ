using UnityEngine;
using UnityEngine.Rendering;

using GamePlay.Camera;
using GamePlay.Camera.Contract;
using GamePlay.Character;
using GamePlay.Character.Contract;
using GamePlay.Collider;
using GamePlay.Collider.Contract;
using GamePlay.Input.Contract;
using SPFramework;

namespace GamePlay.Root
{
    public enum GameRenderFrameRate
    {
        Fps60 = 60,
        Fps120 = 120
    }

    /// <summary>
    /// 游戏启动引导
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class GameEntry : MonoBehaviour
    {
        [Header("渲染设置")]
        [SerializeField, Tooltip("选择渲染帧率")]
        private GameRenderFrameRate _renderFrameRate = GameRenderFrameRate.Fps120;

        private SceneModule _sceneModule;
        private CharacterModule _characterModule;
        private ColliderModule _colliderModule;
        private CameraModule _cameraModule;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // SetRenderFrameRate(_renderFrameRate);

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;

            _sceneModule = new SceneModule();
            ServiceHub.Register<ISceneModule>(_sceneModule);

            _characterModule = new CharacterModule();
            ServiceHub.Register<ICharacterModule>(_characterModule);

            _colliderModule = new ColliderModule();
            ServiceHub.Register<IColliderModule>(_colliderModule);

            _cameraModule = new CameraModule();
            ServiceHub.Register<ICameraModule>(_cameraModule);
        }

        private void Update()
        {
            float elapsedSeconds = Time.deltaTime;

            if (ServiceHub.TryGet<IIputData>(out IIputData inputData))
            {
                inputData.Capture(elapsedSeconds);
            }

            ExecuteLogicUpdate(elapsedSeconds);
        }

        /// <summary>
        /// 游戏世界逻辑逐帧更新
        /// </summary>
        /// <param name="elapsedSeconds">本帧经过的时间 单位为秒</param>
        private void ExecuteLogicUpdate(float elapsedSeconds)
        {
            if (ServiceHub.TryGet<IPlayerInputRouter>(out IPlayerInputRouter playerInputRouter))
            {
                playerInputRouter.LogicUpdate(elapsedSeconds);
            }

            _characterModule.LogicUpdate(elapsedSeconds);
            _colliderModule.LogicUpdate(elapsedSeconds);
        }

        private void LateUpdate()
        {
            _characterModule.RenderUpdate(Time.deltaTime);
            _cameraModule.RenderUpdate(Time.deltaTime);
        }

        private void Start()
        {
            _sceneModule.LoadScene(SceneNames.Gameplay);
        }

        /// <summary>
        /// 设置运行时渲染帧率
        /// </summary>
        private void SetRenderFrameRate(GameRenderFrameRate renderFrameRate)
        {
            _renderFrameRate = renderFrameRate;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = (int)GameRenderFrameRate.Fps120;
            OnDemandRendering.renderFrameInterval = renderFrameRate == GameRenderFrameRate.Fps60 ? 2 : 1;
        }

        private void OnDestroy()
        {
            _sceneModule.Dispose();

            ServiceHub.Unregister<ICameraModule>(_cameraModule);
            ServiceHub.Unregister<IColliderModule>(_colliderModule);
            ServiceHub.Unregister<ICharacterModule>(_characterModule);
            ServiceHub.Unregister<ISceneModule>(_sceneModule);
        }
    }
}
