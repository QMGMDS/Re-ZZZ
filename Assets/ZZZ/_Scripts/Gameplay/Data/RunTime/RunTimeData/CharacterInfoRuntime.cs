using System;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 角色的运行时信息
    /// </summary>
    public sealed class CharacterInfoRuntime
    {
        public int BaseHp { get; }
        public int BaseAtk { get; }
        public CharacterFaction Faction { get; }
        public CharacterFact Fact { get; set; }
        public CharacterIntention Intention { get; set; }
        public Vector2 MoveDirection { get; set; }

        public CharacterInfoRuntime(CharacterInfoAsset characterInfoAsset)
        {
            if (characterInfoAsset == null)
            {
                throw new ArgumentNullException(nameof(characterInfoAsset));
            }

            BaseHp = characterInfoAsset.BaseHp;
            BaseAtk = characterInfoAsset.BaseAtk;
            Faction = characterInfoAsset.Faction;
            Fact = new CharacterFact(Trilean.False);
            Intention = new CharacterIntention(
                Trilean.False,
                Trilean.False,
                Trilean.False,
                Trilean.False,
                Trilean.False);
            MoveDirection = Vector2.zero;
        }
    }
}
