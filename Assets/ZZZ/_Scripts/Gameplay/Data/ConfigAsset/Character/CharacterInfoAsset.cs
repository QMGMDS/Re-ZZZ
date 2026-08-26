using UnityEngine;

namespace GamePlay.Data
{
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

        public int BaseHp => _baseHp;
        public int BaseAtk => _baseAtk;
    }
}
