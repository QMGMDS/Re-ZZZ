using System;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 经过角色输入处理后的本帧输入数据
    /// </summary>
    [Serializable]
    public struct CharacterInputData
    {
        public bool Attack { get; }
        public Vector2 Move { get; }

        public CharacterInputData(bool attack, Vector2 move)
        {
            Attack = attack;
            Move = move;
        }
    }
}
