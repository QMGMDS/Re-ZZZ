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

        public Trilean Death => _death;

        public CharacterFact(Trilean death)
        {
            _death = death;
        }
    }
}
