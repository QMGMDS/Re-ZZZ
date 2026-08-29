namespace GamePlay.Collider.Contract
{
    /// <summary>
    /// 战斗攻击碰撞体控制契约
    /// </summary>
    public interface ICombatCollider
    {
        /// <summary>
        /// 打开攻击碰撞体
        /// </summary>
        /// <param name="entityId">攻击角色的实体 ID</param>
        void OpenAttackCollider(int entityId);

        /// <summary>
        /// 关闭攻击碰撞体
        /// </summary>
        void CloseAttackCollider();
    }
}
