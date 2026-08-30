using System;
using System.Collections.Generic;

namespace SPFramework
{
    /// <summary>
    /// 运行时事实事件总线
    /// </summary>
    public sealed class EventBus : IEventBus, IDisposable
    {
        private readonly Dictionary<Type, IEventChannel> _channels = new Dictionary<Type, IEventChannel>();

        private bool _isDisposed;

        /// <inheritdoc/>
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            EnsureNotDisposed();

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type eventType = typeof(TEvent);

            if (!_channels.TryGetValue(eventType, out IEventChannel rawChannel))
            {
                EventChannel<TEvent> channel =
                    new EventChannel<TEvent>(RemoveChannel);
                _channels.Add(eventType, channel);
                rawChannel = channel;
            }

            return ((EventChannel<TEvent>)rawChannel).Subscribe(handler);
        }

        /// <inheritdoc/>
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            EnsureNotDisposed();

            if (!_channels.TryGetValue(
                    typeof(TEvent),
                    out IEventChannel rawChannel))
            {
                return;
            }

            ((EventChannel<TEvent>)rawChannel).Publish(eventData);
        }

        /// <summary>
        /// 销毁事件总线并清理全部订阅
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            foreach (IEventChannel channel in _channels.Values)
            {
                channel.Clear();
            }

            _channels.Clear();
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    nameof(EventBus),
                    "事件总线已销毁 不能继续订阅或发布事件");
            }
        }

        private void RemoveChannel(Type eventType, IEventChannel channel)
        {
            if (_channels.TryGetValue(eventType, out IEventChannel currentChannel)
                && ReferenceEquals(currentChannel, channel))
            {
                _channels.Remove(eventType);
            }
        }

        private interface IEventChannel
        {
            void Clear();
        }

        private sealed class EventChannel<TEvent> : IEventChannel
            where TEvent : struct, IEvent
        {
            private readonly Action<Type, IEventChannel> _removeChannel;

            private Action<TEvent> _handlers;

            public EventChannel(Action<Type, IEventChannel> removeChannel)
            {
                if (removeChannel == null)
                {
                    throw new ArgumentNullException(nameof(removeChannel));
                }

                _removeChannel = removeChannel;
            }

            public IDisposable Subscribe(Action<TEvent> handler)
            {
                if (_handlers != null
                    && ContainsHandler(handler))
                {
                    throw new InvalidOperationException(
                        $"{typeof(TEvent).Name} 事件处理器不能重复订阅");
                }

                _handlers += handler;
                return new EventSubscription<TEvent>(this, handler);
            }

            public void Publish(TEvent eventData)
            {
                Action<TEvent> handlers = _handlers;
                handlers?.Invoke(eventData);
            }

            public void Unsubscribe(Action<TEvent> handler)
            {
                if (_handlers == null)
                {
                    return;
                }

                _handlers -= handler;

                if (_handlers == null)
                {
                    _removeChannel(typeof(TEvent), this);
                }
            }

            public void Clear()
            {
                _handlers = null;
            }

            private bool ContainsHandler(Action<TEvent> handler)
            {
                Delegate[] handlers = _handlers.GetInvocationList();

                for (int index = 0; index < handlers.Length; index++)
                {
                    if (handlers[index].Equals(handler))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private sealed class EventSubscription<TEvent> : IDisposable
            where TEvent : struct, IEvent
        {
            private EventChannel<TEvent> _channel;
            private Action<TEvent> _handler;

            public EventSubscription(
                EventChannel<TEvent> channel,
                Action<TEvent> handler)
            {
                _channel = channel;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_channel == null)
                {
                    return;
                }

                EventChannel<TEvent> channel = _channel;
                Action<TEvent> handler = _handler;
                _channel = null;
                _handler = null;

                channel.Unsubscribe(handler);
            }
        }
    }
}
