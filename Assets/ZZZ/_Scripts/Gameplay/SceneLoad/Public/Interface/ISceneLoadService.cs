using SPFramework;

namespace GamePlay.SceneLoad.Public
{
    /// <summary>
    /// 场景加载服务契约
    /// </summary>
    public interface ISceneLoadService : IService
    {
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        void SyncLoadScene(string sceneName);
    }
}
