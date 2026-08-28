namespace GamePlay.Character.Contract
{
    /// <summary>
    /// 角色实例更新模块契约
    /// </summary>
    public interface ICharacterModule
    {
        /// <summary>
        /// 注册一个角色更新目标
        /// </summary>
        void Register(ICharacterUpdateTarget target);

        /// <summary>
        /// 注销一个角色更新目标
        /// </summary>
        void Unregister(ICharacterUpdateTarget target);

        /// <summary>
        /// 驱动全部角色执行固定逻辑更新
        /// </summary>
        void LogicUpdate(float tickDeltaSeconds);

        /// <summary>
        /// 驱动全部角色执行宿主帧表现更新
        /// </summary>
        void RenderUpdate(float deltaTimeSeconds);
    }
}
