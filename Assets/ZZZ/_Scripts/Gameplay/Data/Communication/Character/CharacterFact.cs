using System;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 角色当前已经成立的客观事实
    /// </summary>
    [Serializable]
    public struct CharacterFact
    {
        [SerializeField] private Trilean _death;
        [SerializeField] private Trilean _logicalProgress;

        public Trilean Death => _death;
        public Trilean LogicalProgress => _logicalProgress;

        public CharacterFact(Trilean death, Trilean logicalProgress)
        {
            _death = death;
            _logicalProgress = logicalProgress;
        }

        internal void SetLogicalProgress(Trilean logicalProgress)
        {
            _logicalProgress = logicalProgress;
        }
    }
}
