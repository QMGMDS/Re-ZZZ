using System.Collections.Generic;

using UnityEngine;

using SPFramework;

namespace GamePlay.Character.Public
{
    /// <summary>玩家队伍服务契约</summary>
    public interface IPlayerTeamService : IService
    {
        /// <summary>本队伍的全部角色信息</summary>
        IReadOnlyList<CharacterInfo> CharacterInfos { get; }

        /// <summary>当前激活玩家角色的队伍索引</summary>
        int CurrentActiveCharacterIndex { get; }

        /// <summary>当前激活玩家角色的 Transform</summary>
        Transform CurrentActiveCharacterTransform { get; }
    }
}
