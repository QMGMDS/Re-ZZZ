using System;

using UnityEngine;

namespace GamePlay.Character
{
    [CreateAssetMenu(fileName = "CharacterInfo", menuName = "ZZZ/角色/角色信息资产")]
    public sealed class CharacterInfoAsset : ScriptableObject
    {
        [SerializeField]
        private string _characterConfigId;
        [SerializeField, Min(1)]
        private int _baseHealth;
        [SerializeField, Min(0)]
        private int _baseAttack;

        public string CharacterConfigId => _characterConfigId;

        public int BaseHealth => _baseHealth;

        public int BaseAttack => _baseAttack;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_characterConfigId))
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterInfoAsset)} 的 {nameof(CharacterConfigId)} 不能为空");
            }

            if (_baseHealth <= 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterInfoAsset)} 的 {nameof(BaseHealth)} 必须大于零");
            }

            if (_baseAttack < 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterInfoAsset)} 的 {nameof(BaseAttack)} 不能小于零");
            }
        }
    }
}
