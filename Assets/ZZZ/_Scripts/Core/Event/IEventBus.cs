using System;

namespace SPFramework
{
    /// <summary>
    /// 运行时事实事件广播服务
    /// </summary>
    public interface IEventBus : IService
    {
        /// <summary>
        /// 订阅指定类型的事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        /// <returns>用于取消订阅的句柄</returns>
        IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;

        /// <summary>
        /// 发布指定类型的事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="eventData">事件数据</param>
        void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
    }
}
