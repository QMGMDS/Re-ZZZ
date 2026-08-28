using UnityEngine;

using GamePlay.Data;

namespace GamePlay.GameModel
{
    /// <summary>
    /// 缓存宿主帧输入并为逻辑 Tick 提供单次消费
    /// </summary>
    public sealed class InputCommandBuffer
    {
        private Vector2 _latestMove;
        private bool _attackPending;
        private bool _evadePending;
        private bool _skillPending;
        private bool _ultimatePending;
        private bool _switchPending;

        /// <summary>
        /// 写入一次宿主帧输入
        /// </summary>
        public void Capture(in CharacterInputData inputData)
        {
            _latestMove = inputData.Move;
            _attackPending |= inputData.Attack;
            _evadePending |= inputData.Evade;
            _skillPending |= inputData.Skill;
            _ultimatePending |= inputData.Ultimate;
            _switchPending |= inputData.Switch;
        }

        /// <summary>
        /// 读取一次逻辑 Tick 输入并清除单次按键
        /// </summary>
        public CharacterInputData Consume()
        {
            CharacterInputData inputData = new CharacterInputData(
                _latestMove,
                _attackPending,
                _evadePending,
                _skillPending,
                _ultimatePending,
                _switchPending);

            _attackPending = false;
            _evadePending = false;
            _skillPending = false;
            _ultimatePending = false;
            _switchPending = false;
            return inputData;
        }

        /// <summary>
        /// 清除缓存输入
        /// </summary>
        public void Reset()
        {
            _latestMove = Vector2.zero;
            _attackPending = false;
            _evadePending = false;
            _skillPending = false;
            _ultimatePending = false;
            _switchPending = false;
        }
    }
}
