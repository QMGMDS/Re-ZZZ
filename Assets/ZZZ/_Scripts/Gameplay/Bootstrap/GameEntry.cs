using System;

using UnityEngine;

using SPFramework;
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
        private ISceneLoadService _sceneLoadController;

        // 状态标识
        private bool _isGameplaySceneLoaded;

        // 事件订阅句柄
        private IDisposable _sceneLoadCompletedSubscription;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

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
            _sceneLoadCompletedSubscription.Dispose();
        }

        #region 事件回调

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent sceneLoadCompletedEvent)
        {
            _isGameplaySceneLoaded = sceneLoadCompletedEvent.SceneName == SceneNames.Gameplay;
        }

        #endregion
    }
}
