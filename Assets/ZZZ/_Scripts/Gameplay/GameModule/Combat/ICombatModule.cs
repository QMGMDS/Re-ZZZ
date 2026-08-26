using GamePlay.Data;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 战斗系统契约
    /// </summary>
    public interface ICombatModule
    {
        /// <summary>
        /// 提交一次角色攻击请求
        /// </summary>
        void SubmitAttack(AttackRequest attackRequest);
    }
}
