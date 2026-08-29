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

        public Trilean Death => _death;
        public Trilean Hit => _hit;

        public CharacterFact(Trilean death, Trilean hit)
        {
            _death = death;
            _hit = hit;
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
    }
}
