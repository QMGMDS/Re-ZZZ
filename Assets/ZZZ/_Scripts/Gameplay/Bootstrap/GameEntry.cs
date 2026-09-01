using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using GamePlay.Camera.Public;
using GamePlay.Character;
using GamePlay.Character.Contract;
using GamePlay.Combat;
using GamePlay.Combat.Contract;
using GamePlay.Collider;
using GamePlay.Collider.Contract;
using GamePlay.Input.Public;
using GamePlay.Team.Contract;
using SPFramework;

namespace GamePlay.Root
{
    /// <summary>
    /// 游戏启动引导
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class GameEntry : MonoBehaviour
    {
        private SceneModule _sceneModule;
        private CharacterModule _characterModule;
        private CombatModule _combatModule;
        private ColliderModule _colliderModule;
        private PlayerInputRouter _playerInputRouter;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;

            EventBus.Reset();

            _sceneModule = new SceneModule();
            ServiceHub.Register<ISceneModule>(_sceneModule);

            _characterModule = new CharacterModule();
            ServiceHub.Register<ICharacterModule>(_characterModule);

            _combatModule = new CombatModule(_characterModule);
            ServiceHub.Register<ICombatModule>(_combatModule);

            _colliderModule = new ColliderModule();
            ServiceHub.Register<IColliderModule>(_colliderModule);
        }

        private void Start()
        {
            _sceneModule.SceneLoaded += OnSceneLoaded;
            _sceneModule.LoadScene(SceneNames.Gameplay);
        }

        private void Update()
        {
            if (ServiceHub.TryGet<IInputService>(out IInputService inputService))
            {
                inputService.InputCapture();
            }

            if (_playerInputRouter != null && TryGetCurrentCharacter(out ITeamCharacter currentCharacter))
            {
                InputCharacterData inputCharacterData = _playerInputRouter.BuildPlayerInput(Time.deltaTime);
                currentCharacter.ReceivePlayerInput(inputCharacterData);
            }

            _characterModule.LogicUpdate(Time.deltaTime);
            _colliderModule.LogicUpdate(Time.deltaTime);
        }

        private void LateUpdate()
        {
            _characterModule.RenderUpdate(Time.deltaTime);

            if (ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                cameraService.CameraUpdate();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != SceneNames.Gameplay)
            {
                return;
            }

            _sceneModule.SceneLoaded -= OnSceneLoaded;

            if (!ServiceHub.TryGet<IInputService>(out IInputService inputService)
                || !ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                throw new InvalidOperationException(
                    $"{nameof(IInputService)} 和 {nameof(ICameraService)} 必须在 {nameof(SceneNames.Gameplay)} 场景加载后注册");
            }

            _playerInputRouter = new PlayerInputRouter(inputService, cameraService);
        }

        private void OnDestroy()
        {
            _sceneModule.Dispose();

            EventBus.Shutdown();

            ServiceHub.Unregister<IColliderModule>(_colliderModule);
            ServiceHub.Unregister<ICombatModule>(_combatModule);
            ServiceHub.Unregister<ICharacterModule>(_characterModule);
            ServiceHub.Unregister<ISceneModule>(_sceneModule);
        }

        /// <summary>
        /// 尝试获取已配置队伍的当前角色
        /// </summary>
        private bool TryGetCurrentCharacter(out ITeamCharacter currentCharacter)
        {
            if (!ServiceHub.TryGet<ITeamModule>(out ITeamModule teamModule))
            {
                currentCharacter = null;
                return false;
            }

            currentCharacter = teamModule.CurrentCharacter;
            return true;
        }
    }
}
