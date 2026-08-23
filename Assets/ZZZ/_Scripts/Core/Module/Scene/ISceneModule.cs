using System;

using UnityEngine.SceneManagement;

namespace SPFramework
{
    public interface ISceneModule
    {
        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        event Action<Scene, LoadSceneMode> SceneLoaded;

        /// <summary>
        /// 当前主场景名称
        /// </summary>
        string CurrentMainSceneName { get; }

        /// <summary>
        /// 判断指定场景是否为当前主场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        bool IsMainScene(string sceneName);

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="loadSceneMode">场景加载模式</param>
        void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single);
    }
}
