using System;

using SPFramework;

namespace GamePlay.SceneLoad.Public
{
    /// <summary>
    /// 场景加载完成事件，事件的发布在新场景的 OnEnable 之后
    /// </summary>
    public readonly struct SceneLoadCompletedEvent : IEvent
    {
        /// <summary>
        /// 已完成加载的场景名称
        /// </summary>
        public string SceneName { get; }

        /// <summary>
        /// 创建场景加载完成事件
        /// </summary>
        /// <param name="sceneName">已完成加载的场景名称</param>
        public SceneLoadCompletedEvent(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("场景名称不能为空", nameof(sceneName));
            }

            SceneName = sceneName;
        }
    }
}
