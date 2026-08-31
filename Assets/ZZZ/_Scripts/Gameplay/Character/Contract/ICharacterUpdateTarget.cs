namespace GamePlay.Character.Contract
{
    /// <summary>
    /// 角色更新目标契约
    /// </summary>
    public interface ICharacterUpdateTarget
    {
        /// <summary>
        /// 角色实体编号
        /// </summary>
        int EntityId { get; }

        /// <summary>
        /// 执行一次固定逻辑更新
        /// </summary>
        void LogicUpdate(float tickDeltaSeconds);

        /// <summary>
        /// 执行一次宿主帧表现更新
        /// </summary>
        void RenderUpdate(float deltaTimeSeconds);
    }
}
