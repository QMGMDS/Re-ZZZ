using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色所属阵营
    /// </summary>
    public enum CharacterFaction
    {
        Player = 0,
        Enemy = 1
    }

    /// <summary>
    /// 角色的静态模板信息
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterInfo", menuName = "ZZZ/角色/角色信息资产")]
    public sealed class CharacterInfoAsset : ScriptableObject
    {
        [SerializeField, Tooltip("角色基础血量")]
        private int _baseHp;
        [SerializeField, Tooltip("角色基础攻击力")]
        private int _baseAtk;
        [SerializeField, Tooltip("角色所属阵营")]
        private CharacterFaction _faction;

        public int BaseHp => _baseHp;
        public int BaseAtk => _baseAtk;
        public CharacterFaction Faction => _faction;
    }
}
