using UnityEngine;
using UnityEngine.Rendering;

using GamePlay.Camera;
using GamePlay.Camera.Contract;
using GamePlay.Character;
using GamePlay.Character.Contract;
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
        // 游戏世界逻辑更新帧
        private const int GAME_LOGIC_TICK_RATE = 120;
        // 游戏世界逻辑帧的最大补偿
        private const int MAX_LOGIC_TICKS_PER_HOST_FRAME = 8;

        [Header("渲染设置")]
        [SerializeField, Tooltip("选择渲染帧率")]
        private GameRenderFrameRate _renderFrameRate = GameRenderFrameRate.Fps120;

        // 固定步长时钟 - 游戏世界逻辑更新时钟
        private FixedStepClock _fixedStepClock;

        private SceneModule _sceneModule;
        private CharacterModule _characterModule;
        private CameraModule _cameraModule;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _fixedStepClock = new FixedStepClock(
                GAME_LOGIC_TICK_RATE,
                MAX_LOGIC_TICKS_PER_HOST_FRAME);

            SetRenderFrameRate(_renderFrameRate);

            _sceneModule = new SceneModule();
            ServiceHub.Register<ISceneModule>(_sceneModule);

            _characterModule = new CharacterModule();
            ServiceHub.Register<ICharacterModule>(_characterModule);

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

            _fixedStepClock.Advance(elapsedSeconds, ExecuteFixedTick);
        }

        /// <summary>
        /// 游戏世界逻辑更新
        /// </summary>
        /// <param name="_">过去的逻辑总时间</param>
        private void ExecuteFixedTick(float _)
        {
            _characterModule.LogicUpdate(_fixedStepClock.FixedStepSeconds);
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
            Application.targetFrameRate = GAME_LOGIC_TICK_RATE;
            OnDemandRendering.renderFrameInterval = renderFrameRate == GameRenderFrameRate.Fps60 ? 2 : 1;
        }

        private void OnDestroy()
        {
            _cameraModule.Dispose();
            _characterModule.Dispose();
            _sceneModule.Dispose();

            ServiceHub.Unregister<ICameraModule>(_cameraModule);
            ServiceHub.Unregister<ICharacterModule>(_characterModule);
            ServiceHub.Unregister<ISceneModule>(_sceneModule);
            // ServiceHub.Clear();
        }
    }
}
