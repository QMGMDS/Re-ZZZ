using UnityEngine;

using GamePlay.Camera.Public;
using GamePlay.Character;
using GamePlay.Character.Public;
using GamePlay.Collider;
using GamePlay.Collider.Contract;
using GamePlay.Input.Public;
using GamePlay.SceneLoad;
using GamePlay.SceneLoad.Public;
using SPFramework;

namespace GamePlay.Root
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameEntry : MonoBehaviour
    {
        private SceneLoadController _sceneLoadController;
        private PlayerCharacterServiceRouter _playerCharacterServiceRouter;
        private ColliderModule _colliderModule;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;

            EventBus.Reset();

            _sceneLoadController = new SceneLoadController();
            ServiceHub.Register<ISceneLoadService>(_sceneLoadController);

            _playerCharacterServiceRouter = new PlayerCharacterServiceRouter();
            ServiceHub.Register<IPlayerCharacterService>(_playerCharacterServiceRouter);

            _colliderModule = new ColliderModule();
            ServiceHub.Register<IColliderModule>(_colliderModule);
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

            _playerCharacterServiceRouter.CharacterUpdate();
            _colliderModule.LogicUpdate(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                cameraService.CameraUpdate();
            }
        }

        private void OnDestroy()
        {
            if (_colliderModule != null)
            {
                ServiceHub.Unregister<IColliderModule>(_colliderModule);
                _colliderModule = null;
            }

            if (_playerCharacterServiceRouter != null)
            {
                ServiceHub.Unregister<IPlayerCharacterService>(_playerCharacterServiceRouter);
                _playerCharacterServiceRouter.Dispose();
                _playerCharacterServiceRouter = null;
            }

            if (_sceneLoadController != null)
            {
                ServiceHub.Unregister<ISceneLoadService>(_sceneLoadController);
                _sceneLoadController.Dispose();
                _sceneLoadController = null;
            }

            EventBus.Shutdown();
        }
    }
}
