using System;

using UnityEngine;

using GamePlay.Camera.Public;
using GamePlay.Character;
using GamePlay.Character.Contract;
using GamePlay.Combat;
using GamePlay.Combat.Contract;
using GamePlay.Collider;
using GamePlay.Collider.Contract;
using GamePlay.Input.Public;
using GamePlay.SceneLoad;
using GamePlay.SceneLoad.Public;
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
        private SceneLoadController _sceneLoadController;
        private CharacterModule _characterModule;
        private CombatModule _combatModule;
        private ColliderModule _colliderModule;
        private PlayerInputRouter _playerInputRouter;
        private IDisposable _sceneLoadCompletedSubscription;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;

            EventBus.Reset();

            _sceneLoadController = new SceneLoadController();
            ServiceHub.Register<ISceneLoadService>(_sceneLoadController);

            _characterModule = new CharacterModule();
            ServiceHub.Register<ICharacterModule>(_characterModule);

            _combatModule = new CombatModule(_characterModule);
            ServiceHub.Register<ICombatModule>(_combatModule);

            _colliderModule = new ColliderModule();
            ServiceHub.Register<IColliderModule>(_colliderModule);

            _sceneLoadCompletedSubscription =
                EventBus.Subscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
        }

        private void Start()
        {
            _sceneLoadController.SyncLoadScene(SceneNames.Gameplay);
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

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent eventData)
        {
            if (eventData.SceneName != SceneNames.Gameplay)
            {
                return;
            }

            _sceneLoadCompletedSubscription.Dispose();
            _sceneLoadCompletedSubscription = null;

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
            if (_sceneLoadCompletedSubscription != null)
            {
                _sceneLoadCompletedSubscription.Dispose();
                _sceneLoadCompletedSubscription = null;
            }

            _sceneLoadController.Dispose();

            EventBus.Shutdown();

            ServiceHub.Unregister<IColliderModule>(_colliderModule);
            ServiceHub.Unregister<ICombatModule>(_combatModule);
            ServiceHub.Unregister<ICharacterModule>(_characterModule);
            ServiceHub.Unregister<ISceneLoadService>(_sceneLoadController);
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
