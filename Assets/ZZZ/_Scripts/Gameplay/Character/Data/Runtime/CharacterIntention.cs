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
            ValidateValue(move, nameof(move), true);
            ValidateValue(attack, nameof(attack), true);
            ValidateValue(evade, nameof(evade), true);
            ValidateValue(skill, nameof(skill), true);
            ValidateValue(ultimate, nameof(ultimate), true);

            _move = move;
            _attack = attack;
            _evade = evade;
            _skill = skill;
            _ultimate = ultimate;
        }

        public static CharacterIntention CreateAllFalse()
        {
            return AllFalse;
        }

        public void ValidateRuntime()
        {
            ValidateValue(_move, nameof(Move), false);
            ValidateValue(_attack, nameof(Attack), false);
            ValidateValue(_evade, nameof(Evade), false);
            ValidateValue(_skill, nameof(Skill), false);
            ValidateValue(_ultimate, nameof(Ultimate), false);
        }

        public void ValidateCondition()
        {
            ValidateValue(_move, nameof(Move), true);
            ValidateValue(_attack, nameof(Attack), true);
            ValidateValue(_evade, nameof(Evade), true);
            ValidateValue(_skill, nameof(Skill), true);
            ValidateValue(_ultimate, nameof(Ultimate), true);
        }

        private static void ValidateValue(Trilean value, string fieldName, bool allowDontCare)
        {
            if (!Enum.IsDefined(typeof(Trilean), value)
                || (!allowDontCare && value == Trilean.DontCare))
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterIntention)} 的 {fieldName} 只能是 False 或 True");
            }
        }
    }
}
