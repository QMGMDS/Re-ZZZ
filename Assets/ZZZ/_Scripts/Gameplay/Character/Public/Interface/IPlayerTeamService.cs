using UnityEngine;

using SPFramework;

namespace GamePlay.Character.Public
{
    /// <summary>玩家队伍服务契约</summary>
    public interface IPlayerTeamService : IService
    {
        /// <summary>当前激活玩家角色的 Transform</summary>
        Transform CurrentActiveCharacterTransform { get; }
    }
}
