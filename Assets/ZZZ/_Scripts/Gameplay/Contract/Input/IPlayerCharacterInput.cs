using SPFramework;

namespace GamePlay.Contract
{
    /// <summary>
    /// 玩家输入写入角色运行时数据的契约
    /// </summary>
    public interface IPlayerCharacterInput : IService
    {
        /// <summary>
        /// 将当前输入写入玩家角色运行时数据
        /// </summary>
        void WriteRuntimeData(float logicalTimeSeconds);
    }
}
