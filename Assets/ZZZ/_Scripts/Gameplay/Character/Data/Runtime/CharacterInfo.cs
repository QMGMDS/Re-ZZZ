using UnityEngine;

namespace GamePlay.Character
{
    public sealed class CharacterInfo
    {
        /// <summary>角色配置ID</summary>
        public string CharacterConfigId { get; private set; }
        /// <summary>角色头像</summary>
        public Sprite CharacterAvatar { get; private set; }
        /// <summary>角色基础血量</summary>
        public int BaseHealth { get; private set; }
        /// <summary>角色基础攻击力</summary>
        public int BaseAttack { get; private set; }

        /// <summary>角色实体ID</summary>
        public int EntityId { get; private set; }
        /// <summary>角色当前血量</summary>
        public int CurrentHealth { get; private set; }
        /// <summary>角色当前攻击力</summary>
        public int CurrentAttack { get; private set; }

        public CharacterInfo(CharacterInfoAsset characterInfoAsset, int assignedEntityId)
        {
            CharacterConfigId = characterInfoAsset.CharacterConfigId;
            CharacterAvatar = characterInfoAsset.CharacterAvatar;
            BaseHealth = characterInfoAsset.BaseHealth;
            BaseAttack = characterInfoAsset.BaseAttack;

            CurrentHealth = BaseHealth;
            CurrentAttack = BaseAttack;

            EntityId = assignedEntityId;
        }
    }
}
