using SPFramework;

namespace GamePlay.Combat.Contract
{
    /// <summary>
    /// 战斗判定模块契约
    /// </summary>
    public interface ICombatModule : IService
    {
        /// <summary>
        /// 提交一次碰撞命中请求
        /// </summary>
        /// <param name="attackerEntityId">攻击方角色实体 ID</param>
        /// <param name="targetEntityId">目标方角色实体 ID</param>
        void SubmitHit(int attackerEntityId, int targetEntityId);
    }
}
