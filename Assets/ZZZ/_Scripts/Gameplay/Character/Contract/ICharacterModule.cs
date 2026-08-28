using System.Collections.Generic;

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
        int Register(ICharacterUpdateTarget target, CharacterInfoRuntime characterInfoRuntime);

        /// <summary>
        /// 注销一个角色更新目标
        /// </summary>
        void Unregister(ICharacterUpdateTarget target);

        /// <summary>
        /// 获取当前已注册角色的运行时信息
        /// </summary>
        IReadOnlyDictionary<int, CharacterInfoRuntime> GetCharacterInfoRuntimes();

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
