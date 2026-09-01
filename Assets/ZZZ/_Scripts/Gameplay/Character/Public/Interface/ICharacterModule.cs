using SPFramework;

namespace GamePlay.Character.Contract
{
    /// <summary>
    /// 角色实例更新模块契约
    /// </summary>
    public interface ICharacterModule : IService
    {
        /// <summary>
        /// 注册一个角色更新目标
        /// </summary>
        int Register(
            ICharacterUpdateTarget target,
            ICharacterHurtReceiver hurtReceiver,
            CharacterInfoRuntime characterInfoRuntime);

        /// <summary>
        /// 注销一个角色更新目标
        /// </summary>
        void Unregister(ICharacterUpdateTarget target);

        /// <summary>
        /// 根据实体 ID 获取角色运行时信息
        /// </summary>
        bool TryGetCharacterInfoRuntime(
            int entityId,
            out CharacterInfoRuntime characterInfoRuntime);

        /// <summary>
        /// 根据实体 ID 获取角色受击入口
        /// </summary>
        bool TryGetCharacterHurtReceiver(
            int entityId,
            out ICharacterHurtReceiver hurtReceiver);

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
