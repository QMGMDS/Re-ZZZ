using System;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 角色当前想要执行的行为
    /// </summary>
    [Serializable]
    public struct CharacterIntention
    {
        [SerializeField] private Trilean _attack;
        [SerializeField] private Trilean _move;

        public Trilean Attack => _attack;
        public Trilean Move => _move;

        public CharacterIntention(Trilean attack, Trilean move)
        {
            _attack = attack;
            _move = move;
        }
    }
}
