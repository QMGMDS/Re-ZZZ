using System;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 玩家本帧的原始输入数据
    /// </summary>
    [Serializable]
    public struct RawInputData
    {
        public bool Attack { get; }
        public Vector2 Move { get; }

        public RawInputData(bool attack, Vector2 move)
        {
            Attack = attack;
            Move = move;
        }
    }
}
