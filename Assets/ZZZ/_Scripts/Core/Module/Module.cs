namespace SPFramework
{
    /// <summary>
    /// 模块抽象基类
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// 获取模块优先级
        /// </summary>
        /// <remarks>优先级高的模块会优先轮询 并且关闭操作会后执行</remarks>
        public virtual int Priority => 0;

        /// <summary>
        /// 创建模块
        /// </summary>
        public abstract void OnCreate();

        /// <summary>
        /// 销毁并清理模块
        /// </summary>
        public abstract void OnDestroy();
    }
}
