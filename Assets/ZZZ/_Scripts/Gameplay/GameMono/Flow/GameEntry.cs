using System;

using UnityEngine;
using UnityEngine.Rendering;

using GamePlay.Contract;
using GamePlay.Data;
using GamePlay.GameModule;
using GamePlay.GameModel;
using SPFramework;

namespace GamePlay.GameMono
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
        private const int GAME_LOGIC_TICK_RATE = 120;
        private const int MAX_LOGIC_TICKS_PER_HOST_FRAME = 8;

        [Header("渲染设置")]
        [SerializeField, Tooltip("选择渲染帧率")]
        private GameRenderFrameRate _renderFrameRate = GameRenderFrameRate.Fps120;

        private FixedStepClock _fixedStepClock;
        private Action<float> _fixedTick;
        private ICharacterModule _characterModule;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _fixedStepClock = new FixedStepClock(
                GAME_LOGIC_TICK_RATE,
                MAX_LOGIC_TICKS_PER_HOST_FRAME);

            _fixedTick = ExecuteFixedTick;

            SetRenderFrameRate(_renderFrameRate);

            ModuleSystem.RegisterModule<IEntityModule>(new EntityModule());
            _characterModule = ModuleSystem.RegisterModule<ICharacterModule>(new CharacterModule());
            ModuleSystem.RegisterModule<ICombatModule>(new CombatModule());
            ModuleSystem.RegisterModule<ISceneModule>(new SceneModule());
        }

        private void Start()
        {
            ModuleSystem.GetModule<ISceneModule>().LoadScene(SceneNames.Gameplay);
        }

        private void Update()
        {
            float elapsedSeconds = Time.deltaTime;

            if (ServiceHub.TryGet<IIputData>(out IIputData inputData))
            {
                inputData.Capture(elapsedSeconds);
            }

            _fixedStepClock.Advance(elapsedSeconds, _fixedTick);
        }

        private void LateUpdate()
        {
            _characterModule.RenderUpdate(Time.deltaTime);
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

        private void ExecuteFixedTick(float logicalTimeSeconds)
        {
            if (ServiceHub.TryGet<IPlayerCharacterInput>(out IPlayerCharacterInput playerCharacterInput))
            {
                playerCharacterInput.WriteRuntimeData(logicalTimeSeconds);
            }

            _characterModule.LogicUpdate(_fixedStepClock.FixedStepSeconds);
        }

        private void OnDestroy()
        {
            ModuleSystem.Destroy();
        }
    }
}
