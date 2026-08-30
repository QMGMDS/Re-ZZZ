using System;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色当前已经成立的客观事实
    /// </summary>
    [Serializable]
    public struct CharacterFact
    {
        [SerializeField] private Trilean _death;
        [SerializeField] private Trilean _hit;
        [SerializeField] private Trilean _switchIn;
        [SerializeField] private Trilean _switchOut;

        public Trilean Death => _death;
        public Trilean Hit => _hit;
        public Trilean SwitchIn => _switchIn;
        public Trilean SwitchOut => _switchOut;

        public CharacterFact(
            Trilean death,
            Trilean hit,
            Trilean switchIn,
            Trilean switchOut)
        {
            _death = death;
            _hit = hit;
            _switchIn = switchIn;
            _switchOut = switchOut;
        }

        public CharacterFact MarkHit()
        {
            _hit = Trilean.True;
            return this;
        }

        public CharacterFact ConsumeHit()
        {
            _hit = Trilean.False;
            return this;
        }

        public CharacterFact MarkSwitchIn()
        {
            _switchIn = Trilean.True;
            return this;
        }

        public CharacterFact ConsumeSwitchIn()
        {
            _switchIn = Trilean.False;
            return this;
        }

        public CharacterFact MarkSwitchOut()
        {
            _switchOut = Trilean.True;
            return this;
        }

        public CharacterFact ConsumeSwitchOut()
        {
            _switchOut = Trilean.False;
            return this;
        }
    }
}
