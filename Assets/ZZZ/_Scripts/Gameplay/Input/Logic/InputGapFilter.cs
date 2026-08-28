using System;

using UnityEngine;

namespace GamePlay.Input
{
    /// <summary>
    /// 修复移动输入方向切换时的短暂空窗
    /// </summary>
    public sealed class InputGapFilter
    {
        private readonly float _gapToleranceSeconds;

        private float _zeroElapsedSeconds;
        private Vector2 _lastValidAxis;

        public InputGapFilter(float gapToleranceSeconds)
        {
            if (gapToleranceSeconds < 0f
                || float.IsNaN(gapToleranceSeconds)
                || float.IsInfinity(gapToleranceSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(gapToleranceSeconds));
            }

            _gapToleranceSeconds = gapToleranceSeconds;
        }

        /// <summary>
        /// 过滤移动输入方向切换时的短暂零输入
        /// </summary>
        public Vector2 FilterAxis(Vector2 input, float deltaTime)
        {
            if (deltaTime < 0f
                || float.IsNaN(deltaTime)
                || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (input.sqrMagnitude != 0f)
            {
                _lastValidAxis = input;
                _zeroElapsedSeconds = 0f;
                return input;
            }

            if (_lastValidAxis.sqrMagnitude == 0f)
            {
                return Vector2.zero;
            }

            _zeroElapsedSeconds += deltaTime;
            if (_gapToleranceSeconds > 0f
                && _zeroElapsedSeconds <= _gapToleranceSeconds)
            {
                return _lastValidAxis;
            }

            _lastValidAxis = Vector2.zero;
            _zeroElapsedSeconds = 0f;
            return Vector2.zero;
        }

        /// <summary>
        /// 清除当前过滤状态
        /// </summary>
        public void Reset()
        {
            _zeroElapsedSeconds = 0f;
            _lastValidAxis = Vector2.zero;
        }
    }
}
