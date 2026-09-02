using System;

using UnityEngine;

using GamePlay.Definition;

namespace GamePlay.Character
{
    [Serializable]
    public struct CharacterFact
    {
        [SerializeField]
        private Trilean _switchIn;
        [SerializeField]
        private Trilean _switchOut;
        [SerializeField]
        private Trilean _hit;
        [SerializeField]
        private Trilean _death;

        public Trilean SwitchIn => _switchIn;
        public Trilean SwitchOut => _switchOut;
        public Trilean Hit => _hit;
        public Trilean Death => _death;

        public static CharacterFact AllFalse => new CharacterFact(
            Trilean.False,
            Trilean.False,
            Trilean.False,
            Trilean.False);

        public CharacterFact(
            Trilean enterField,
            Trilean exitField,
            Trilean hit,
            Trilean death)
        {
            _switchIn = enterField;
            _switchOut = exitField;
            _hit = hit;
            _death = death;
        }

        public CharacterFact MarkEnterField()
        {
            _switchIn = Trilean.True;
            return this;
        }

        public CharacterFact MarkExitField()
        {
            _switchOut = Trilean.True;
            return this;
        }

        public CharacterFact MarkHit()
        {
            _hit = Trilean.True;
            return this;
        }

        public CharacterFact MarkDeath()
        {
            _death = Trilean.True;
            return this;
        }

        public CharacterFact ConsumeRequired(CharacterFact requiredFact)
        {
            if (requiredFact._switchIn == Trilean.True && _switchIn == Trilean.True)
            {
                _switchIn = Trilean.False;
            }

            if (requiredFact._switchOut == Trilean.True && _switchOut == Trilean.True)
            {
                _switchOut = Trilean.False;
            }

            if (requiredFact._hit == Trilean.True && _hit == Trilean.True)
            {
                _hit = Trilean.False;
            }

            if (requiredFact._death == Trilean.True && _death == Trilean.True)
            {
                _death = Trilean.False;
            }

            return this;
        }
    }
}
