using SPFramework;

namespace GamePlay.Camera.Contract
{
    /// <summary>
    /// 摄像机更新模块契约
    /// </summary>
    public interface ICameraModule : IService
    {
        /// <summary>
        /// 执行一次摄像机宿主帧更新
        /// </summary>
        void RenderUpdate(float deltaTimeSeconds);
    }
}
