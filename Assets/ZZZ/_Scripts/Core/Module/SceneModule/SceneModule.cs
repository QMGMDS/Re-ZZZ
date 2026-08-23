using System;

using UnityEngine.SceneManagement;

namespace SPFramework
{
    public sealed class SceneModule : Module, ISceneModule
    {
        /// <inheritdoc/>
        public override void OnCreate()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <inheritdoc/>
        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region ISceneModule

        /// <inheritdoc/>
        public event Action<Scene, LoadSceneMode> SceneLoaded;

        /// <inheritdoc/>
        public string CurrentMainSceneName => SceneManager.GetActiveScene().name;

        /// <inheritdoc/>
        public bool IsMainScene(string sceneName)
        {
            return CurrentMainSceneName == sceneName;
        }

        /// <inheritdoc/>
        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("场景名称不能为空", nameof(sceneName));
            }

            SceneManager.LoadScene(sceneName, loadSceneMode);
        }

        /* 回调执行时机 
            简单来说 一个场景加载时的完整生命周期顺序大致如下：
            所有活动 GameObject 的 Awake 方法被调用
            所有活动 GameObject 的 OnEnable 方法被调用
            SceneManager.sceneLoaded 事件触发 OnSceneLoaded 回调在此执行
            所有活动 GameObject 的 Start 方法被调用
        */
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneLoaded?.Invoke(scene, loadSceneMode);
        }

        #endregion
    }
}
