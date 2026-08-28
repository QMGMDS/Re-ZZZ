using System;

using UnityEngine;

namespace GamePlay.Input
{
    /// <summary>
    /// 经过角色输入处理后的本帧输入数据
    /// </summary>
    [Serializable]
    public struct CharacterInputData
    {
        public bool Attack { get; }
        public Vector2 Move { get; }
        public bool Evade { get; }
        public bool Skill { get; }
        public bool Ultimate { get; }
        public bool Switch { get; }

        public CharacterInputData(
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
