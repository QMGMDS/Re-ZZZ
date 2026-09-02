using System;

using UnityEngine;

using GamePlay.Definition;

namespace GamePlay.Character
{
    [Serializable]
    public struct CharacterIntention
    {
        [SerializeField]
        private Trilean _move;
        [SerializeField]
        private Trilean _attack;
        [SerializeField]
        private Trilean _evade;
        [SerializeField]
        private Trilean _skill;
        [SerializeField]
        private Trilean _ultimate;

        public Trilean Move => _move;
        public Trilean Attack => _attack;
        public Trilean Evade => _evade;
        public Trilean Skill => _skill;
        public Trilean Ultimate => _ultimate;

        public static CharacterIntention AllFalse => new CharacterIntention(
            Trilean.False,
            Trilean.False,
            Trilean.False,
            Trilean.False,
            Trilean.False);

        public CharacterIntention(
            Trilean move,
            Trilean attack,
            Trilean evade,
            Trilean skill,
            Trilean ultimate)
        {
            _move = move;
            _attack = attack;
            _evade = evade;
            _skill = skill;
            _ultimate = ultimate;
        }
    }
}
