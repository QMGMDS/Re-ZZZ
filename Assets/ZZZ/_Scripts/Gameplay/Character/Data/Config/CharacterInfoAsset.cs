using UnityEngine;

namespace GamePlay.Character
{
    [CreateAssetMenu(fileName = "CharacterInfo", menuName = "ZZZ/角色/角色信息资产")]
    public sealed class CharacterInfoAsset : ScriptableObject
    {
        [SerializeField, Tooltip("角色信息ID")]
        private string _characterConfigId;
        [SerializeField, Tooltip("角色头像")]
        private Sprite _characterAvatar;
        [SerializeField, Min(1), Tooltip("基础血量")]
        private int _baseHealth = 1;
        [SerializeField, Min(0), Tooltip("基础攻击")]
        private int _baseAttack = 0;

        public string CharacterConfigId => _characterConfigId;
        public Sprite CharacterAvatar => _characterAvatar;
        public int BaseHealth => _baseHealth;
        public int BaseAttack => _baseAttack;
    }
}
