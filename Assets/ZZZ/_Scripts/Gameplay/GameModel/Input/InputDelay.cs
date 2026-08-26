using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.GameModel
{
    /// <summary>
    /// 输入数据延时器
    /// </summary>
    public sealed class InputDelay
    {
        private readonly float _delaySeconds;
        private readonly Queue<DelayedInput<bool>> _buttonBuffer =
            new Queue<DelayedInput<bool>>();
        private readonly Queue<DelayedInput<Vector2>> _axisBuffer =
            new Queue<DelayedInput<Vector2>>();

        private float _buttonElapsedSeconds;
        private float _axisElapsedSeconds;
        private bool _delayedButton;
        private Vector2 _delayedAxis;

        public InputDelay(float delaySeconds)
        {
            if (delaySeconds < 0f
                || float.IsNaN(delaySeconds)
                || float.IsInfinity(delaySeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(delaySeconds));
            }

            _delaySeconds = delaySeconds;
        }

        /// <summary>
        /// 延时按键输入
        /// </summary>
        public bool DelayButton(bool input, float deltaTime)
        {
            if (deltaTime < 0f
                || float.IsNaN(deltaTime)
                || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (_delaySeconds == 0f)
            {
                return input;
            }

            _buttonBuffer.Enqueue(new DelayedInput<bool>(_buttonElapsedSeconds, input));
            _buttonElapsedSeconds += deltaTime;

            float targetTime = _buttonElapsedSeconds - _delaySeconds;
            while (_buttonBuffer.Count > 0
                && _buttonBuffer.Peek().Timestamp <= targetTime)
            {
                _delayedButton = _buttonBuffer.Dequeue().Value;
            }

            return _delayedButton;
        }

        /// <summary>
        /// 延时输入轴
        /// </summary>
        public Vector2 DelayAxis(Vector2 input, float deltaTime)
        {
            if (deltaTime < 0f
                || float.IsNaN(deltaTime)
                || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (_delaySeconds == 0f)
            {
                return input;
            }

            _axisBuffer.Enqueue(new DelayedInput<Vector2>(_axisElapsedSeconds, input));
            _axisElapsedSeconds += deltaTime;

            float targetTime = _axisElapsedSeconds - _delaySeconds;
            while (_axisBuffer.Count > 0
                && _axisBuffer.Peek().Timestamp <= targetTime)
            {
                _delayedAxis = _axisBuffer.Dequeue().Value;
            }

            return _delayedAxis;
        }

        private readonly struct DelayedInput<T>
        {
            public readonly float Timestamp;
            public readonly T Value;

            public DelayedInput(float timestamp, T value)
            {
                Timestamp = timestamp;
                Value = value;
            }
        }
    }
}
