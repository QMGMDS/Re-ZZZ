using System;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 指定逻辑时刻输入给角色的数据
    /// </summary>
    [Serializable]
    public struct InputCharacterData
    {
        // 本次传入输入数据的时刻
        public float LogicalTime { get; }
        // 传入的角色意图
        public CharacterIntention Intention { get; }
        // 传入的移动输入
        public Vector2 MoveInput { get; }

        public InputCharacterData(float logicalTime, CharacterIntention intention, Vector2 moveInput)
        {
            LogicalTime = logicalTime;
            Intention = intention;
            MoveInput = moveInput;
        }
    }
}
