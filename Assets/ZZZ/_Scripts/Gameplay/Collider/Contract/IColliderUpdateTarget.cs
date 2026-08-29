namespace GamePlay.Collider.Contract
{
    /// <summary>
    /// 碰撞体更新目标契约
    /// </summary>
    public interface IColliderUpdateTarget
    {
        /// <summary>
        /// 执行一次固定逻辑更新
        /// </summary>
        void LogicUpdate(float tickDeltaSeconds);
    }
}
