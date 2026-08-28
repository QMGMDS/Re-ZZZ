using SPFramework;

namespace GamePlay.Character.Contract
{
    /// <summary>
    /// 玩家输入路由器契约
    /// </summary>
    public interface IPlayerInputRouter : IService
    {
        /// <summary>
        /// 驱动一次逻辑 Tick 的玩家输入路由
        /// </summary>
        /// <param name="logicalTimeSeconds">当前逻辑时间 单位为秒</param>
        void LogicUpdate(float logicalTimeSeconds);
    }
}
