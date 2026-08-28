using System;

using UnityEngine;

namespace GamePlay.Character
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
        [SerializeField] private Trilean _skill;
        [SerializeField] private Trilean _ultimate;
        [SerializeField] private Trilean _switch;

        public Trilean Move => _move;
        public Trilean Attack => _attack;
        public Trilean Evade => _evade;
        public Trilean Skill => _skill;
        public Trilean Ultimate => _ultimate;
        public Trilean Switch => _switch;

        public CharacterIntention(
            Trilean move,
            Trilean attack,
            Trilean evade,
            Trilean skill,
            Trilean ultimate,
            Trilean switchInput)
        {
            _move = move;
            _attack = attack;
            _evade = evade;
            _skill = skill;
            _ultimate = ultimate;
            _switch = switchInput;
        }
    }
}
