using System;

namespace GamePlay.GameModel
{
    /// <summary>
    /// 以固定时间步长推进逻辑时间
    /// </summary>
    public sealed class FixedStepClock
    {
        private readonly double _fixedStepSeconds;
        private readonly int _maxTicksPerAdvance;

        private double _accumulatedSeconds;
        private double _logicalTimeSeconds;

        public float FixedStepSeconds => (float)_fixedStepSeconds;
        public double LogicalTimeSeconds => _logicalTimeSeconds;
        public double DiscardedSeconds { get; private set; }
        public int LastTickCount { get; private set; }

        public FixedStepClock(int tickRate, int maxTicksPerAdvance)
        {
            if (tickRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tickRate));
            }

            if (maxTicksPerAdvance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTicksPerAdvance));
            }

            _fixedStepSeconds = 1d / tickRate;
            _maxTicksPerAdvance = maxTicksPerAdvance;
        }

        /// <summary>
        /// 累加宿主帧时间并执行有限数量的逻辑 Tick
        /// </summary>
        public int Advance(float elapsedSeconds, Action<float> tickAction)
        {
            if (float.IsNaN(elapsedSeconds)
                || float.IsInfinity(elapsedSeconds)
                || elapsedSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            }

            if (tickAction == null)
            {
                throw new ArgumentNullException(nameof(tickAction));
            }

            LastTickCount = 0;

            _accumulatedSeconds += elapsedSeconds;

            double completeTickCount = Math.Floor(_accumulatedSeconds / _fixedStepSeconds);
            double discardedTickCount = completeTickCount - _maxTicksPerAdvance;
            if (discardedTickCount > 0d)
            {
                double discardedSeconds = discardedTickCount * _fixedStepSeconds;
                DiscardedSeconds += discardedSeconds;
                _accumulatedSeconds -= discardedSeconds;
            }

            while (_accumulatedSeconds >= _fixedStepSeconds
                && LastTickCount < _maxTicksPerAdvance)
            {
                _accumulatedSeconds -= _fixedStepSeconds;
                _logicalTimeSeconds += _fixedStepSeconds;
                LastTickCount++;
                tickAction((float)_logicalTimeSeconds);
            }

            return LastTickCount;
        }
    }
}
