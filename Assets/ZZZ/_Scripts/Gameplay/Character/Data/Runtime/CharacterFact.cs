using System;

using UnityEngine;

using GamePlay.Definition;

namespace GamePlay.Character
{
    [Serializable]
    public struct CharacterFact
    {
        [SerializeField]
        private Trilean _enterField;
        [SerializeField]
        private Trilean _exitField;
        [SerializeField]
        private Trilean _hit;
        [SerializeField]
        private Trilean _death;

        public Trilean EnterField => _enterField;

        public Trilean ExitField => _exitField;

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
            ValidateValue(enterField, nameof(enterField), true);
            ValidateValue(exitField, nameof(exitField), true);
            ValidateValue(hit, nameof(hit), true);
            ValidateValue(death, nameof(death), true);

            _enterField = enterField;
            _exitField = exitField;
            _hit = hit;
            _death = death;
        }

        public static CharacterFact CreateAllFalse()
        {
            return AllFalse;
        }

        public void ValidateRuntime()
        {
            ValidateValue(_enterField, nameof(EnterField), false);
            ValidateValue(_exitField, nameof(ExitField), false);
            ValidateValue(_hit, nameof(Hit), false);
            ValidateValue(_death, nameof(Death), false);
        }

        public void ValidateCondition()
        {
            ValidateValue(_enterField, nameof(EnterField), true);
            ValidateValue(_exitField, nameof(ExitField), true);
            ValidateValue(_hit, nameof(Hit), true);
            ValidateValue(_death, nameof(Death), true);
        }

        public CharacterFact MarkEnterField()
        {
            _enterField = Trilean.True;
            return this;
        }

        public CharacterFact MarkExitField()
        {
            _exitField = Trilean.True;
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
            requiredFact.ValidateCondition();
            ValidateRuntime();

            if (requiredFact._enterField == Trilean.True && _enterField == Trilean.True)
            {
                _enterField = Trilean.False;
            }

            if (requiredFact._exitField == Trilean.True && _exitField == Trilean.True)
            {
                _exitField = Trilean.False;
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

        private static void ValidateValue(Trilean value, string fieldName, bool allowDontCare)
        {
            if (!Enum.IsDefined(typeof(Trilean), value)
                || (!allowDontCare && value == Trilean.DontCare))
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterFact)} 的 {fieldName} 只能是 False 或 True");
            }
        }
    }
}
