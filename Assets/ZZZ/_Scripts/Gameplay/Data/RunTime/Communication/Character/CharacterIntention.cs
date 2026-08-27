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
        [SerializeField] private Trilean _move;
        [SerializeField] private Trilean _attack;
        [SerializeField] private Trilean _evade;

        public Trilean Move => _move;
        public Trilean Attack => _attack;
        public Trilean Evade => _evade;

        public CharacterIntention(Trilean move, Trilean attack, Trilean evade)
        {
            _move = move;
            _attack = attack;
            _evade = evade;
        }
    }
}
