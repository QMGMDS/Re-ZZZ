using SPFramework;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 模块根模型
    /// </summary>
    public class RootModule
    {
        /// <summary>
        /// 创建并注册所有通用模块和业务模块
        /// </summary>
        public void Initialize()
        {
            ModuleSystem.RegisterModule<IEntityModule>(new EntityModule());
            ModuleSystem.RegisterModule<ISceneModule>(new SceneModule());
        }

        /// <summary>
        /// 驱动所有已注册模块。
        /// </summary>
        /// <param name="elapsedSeconds">逻辑时间间隔 单位为秒</param>
        /// <param name="realElapsedSeconds">真实时间间隔 单位为秒</param>
        public void Update(float elapsedSeconds, float realElapsedSeconds)
        {
            ModuleSystem.Update(elapsedSeconds, realElapsedSeconds);
        }

        /// <summary>
        /// 销毁并取消注册所有模块。
        /// </summary>
        public void Destroy()
        {
            ModuleSystem.Destroy();
        }
    }
}
