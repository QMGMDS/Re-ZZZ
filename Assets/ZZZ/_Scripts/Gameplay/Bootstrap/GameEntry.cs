using System;

using UnityEngine;

using SPFramework;
using GamePlay.Character;
using GamePlay.SceneLoad;
using GamePlay.Camera.Public;
using GamePlay.Input.Public;
using GamePlay.SceneLoad.Public;

namespace GamePlay.Root
{
    /// <summary>
    /// 游戏启动引导
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class GameEntry : MonoBehaviour
    {
        private CharacterInfoRegistry _characterInfoRegistry;
        private SceneLoadController _sceneLoadController;

        // 状态标识
        private bool _isGameplaySceneLoaded;

        // 事件退订句柄
        private IDisposable _sceneLoadCompletedSubscription;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _characterInfoRegistry = new CharacterInfoRegistry();
            _sceneLoadController = new SceneLoadController();

            _sceneLoadCompletedSubscription = EventBus.Subscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
        }

        private void Start()
        {
            _sceneLoadController.SyncLoadScene(SceneNames.Gameplay);
        }

        private void Update()
        {
            if (!_isGameplaySceneLoaded)
            {
                return;
            }

            if (ServiceHub.TryGet<IInputService>(out IInputService inputService))
            {
                inputService.InputCapture();
            }

            if (ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                cameraService.CameraUpdate();
            }
        }

        private void OnDestroy()
        {
            // 事件退订
            _sceneLoadCompletedSubscription.Dispose();

            _sceneLoadController.Dispose();
            _characterInfoRegistry.Dispose();
        }

        #region 事件回调

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent sceneLoadCompletedEvent)
        {
            _isGameplaySceneLoaded = sceneLoadCompletedEvent.SceneName == SceneNames.Gameplay;
        }

        #endregion
    }
}
