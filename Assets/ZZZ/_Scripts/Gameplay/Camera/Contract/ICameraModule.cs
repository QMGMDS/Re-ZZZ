using SPFramework;

namespace GamePlay.Camera.Contract
{
    /// <summary>
    /// 摄像机实例更新模块契约
    /// </summary>
    public interface ICameraModule : IService
    {
        /// <summary>
        /// 注册一个摄像机更新目标
        /// </summary>
        void Register(ICameraUpdateTarget target);

        /// <summary>
        /// 注销一个摄像机更新目标
        /// </summary>
        void Unregister(ICameraUpdateTarget target);

        /// <summary>
        /// 驱动全部摄像机执行宿主帧更新
        /// </summary>
        void RenderUpdate(float deltaTimeSeconds);
    }
}
