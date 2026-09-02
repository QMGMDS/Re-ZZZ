using System;

using UnityEngine.SceneManagement;

using GamePlay.SceneLoad.Public;
using SPFramework;

namespace GamePlay.SceneLoad
{
    /// <summary>
    /// 场景加载控制器
    /// </summary>
    public sealed class SceneLoadController : ISceneLoadService
    {
        /// <summary>
        /// 构造场景加载控制器
        /// </summary>
        public SceneLoadController()
        {
            ServiceHub.Register<ISceneLoadService>(this);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// 释放场景加载控制器
        /// </summary>
        public void Dispose()
        {
            ServiceHub.Unregister<ISceneLoadService>(this);

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region 服务接口

        /// <inheritdoc/>
        public void SyncLoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("场景名称不能为空", nameof(sceneName));
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        #endregion

        #region 发布事件

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            EventBus.Publish(new SceneLoadCompletedEvent(scene.name));
        }

        #endregion
    }
}
