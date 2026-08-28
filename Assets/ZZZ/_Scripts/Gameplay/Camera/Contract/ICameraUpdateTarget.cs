namespace GamePlay.Camera.Contract
{
    /// <summary>
    /// 摄像机渲染更新目标契约
    /// </summary>
    public interface ICameraUpdateTarget
    {
        /// <summary>
        /// 执行一次宿主帧摄像机更新
        /// </summary>
        void RenderUpdate(float deltaTimeSeconds);
    }
}
