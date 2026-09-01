using SPFramework;

namespace GamePlay.Collider.Contract
{
    /// <summary>
    /// 碰撞体实例更新模块契约
    /// </summary>
    public interface IColliderModule : IService
    {
        /// <summary>
        /// 注册一个碰撞体更新目标
        /// </summary>
        void Register(IColliderUpdateTarget target);

        /// <summary>
        /// 注销一个碰撞体更新目标
        /// </summary>
        void Unregister(IColliderUpdateTarget target);

        /// <summary>
        /// 驱动全部碰撞体执行固定逻辑更新
        /// </summary>
        void LogicUpdate(float tickDeltaSeconds);
    }
}
