using System;

using UnityEngine;

using GamePlay.Team;

using SPFramework;

namespace GamePlay.Team.Contract
{
    /// <summary>
    /// 队伍角色切换事件
    /// </summary>
    public readonly struct TeamCharacterSwitchedEvent : IEvent
    {
        /// <summary>
        /// 请求接受后的队伍只读快照 不代表退场动画已完成
        /// </summary>
        public TeamReadOnlyInfo TeamInfo { get; }

        /// <summary>
        /// 切换请求中的角色 Transform
        /// </summary>
        public Transform CharacterTransform { get; }

        /// <summary>
        /// 创建队伍角色切换事件
        /// </summary>
        /// <param name="teamInfo">请求接受后的队伍只读快照</param>
        /// <param name="characterTransform">切换请求中的角色 Transform</param>
        public TeamCharacterSwitchedEvent(TeamReadOnlyInfo teamInfo, Transform characterTransform)
        {
            if (teamInfo == null)
            {
                throw new ArgumentNullException(nameof(teamInfo));
            }

            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            TeamInfo = teamInfo;
            CharacterTransform = characterTransform;
        }
    }
}
