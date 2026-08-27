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
        public bool Evade { get; }
        public bool Skill { get; }
        public bool Ultimate { get; }
        public bool Switch { get; }

        public RawInputData(
            Vector2 move,
            bool attack,
            bool evade,
            bool skill,
            bool ultimate,
            bool switchInput)
        {
            Move = move;
            Attack = attack;
            Evade = evade;
            Skill = skill;
            Ultimate = ultimate;
            Switch = switchInput;
        }
    }
}
