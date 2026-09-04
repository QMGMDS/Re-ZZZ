using System;
using System.Collections.Generic;

using SPFramework;
using GamePlay.Character.Public;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色信息注册中心
    /// </summary>
    public sealed class CharacterInfoRegistry : ICharacterInfoRegistryService
    {
        // <角色实体ID,角色实体信息> 字典
        private readonly Dictionary<int, CharacterInfo> _characterInfos = new Dictionary<int, CharacterInfo>();

        private int _nextEntityId;

        /// <summary>
        /// 构造角色信息注册中心
        /// </summary>
        public CharacterInfoRegistry()
        {
            ServiceHub.Register<ICharacterInfoRegistryService>(this);
        }

        /// <summary>
        /// 释放角色信息注册中心
        /// </summary>
        public void Dispose()
        {
            ServiceHub.Unregister<ICharacterInfoRegistryService>(this);
            _characterInfos.Clear();
        }

        #region 服务接口

        /// <inheritdoc/>
        public CharacterInfo RegisterCharacterInfo(CharacterInfoAsset characterInfoAsset)
        {
            if (characterInfoAsset == null)
            {
                throw new ArgumentNullException(nameof(characterInfoAsset));
            }

            int assignedEntityId = _nextEntityId;
            var characterInfo = new CharacterInfo(characterInfoAsset, assignedEntityId);

            _characterInfos.Add(assignedEntityId, characterInfo);
            _nextEntityId++;

            return characterInfo;
        }

        #endregion
    }
}
