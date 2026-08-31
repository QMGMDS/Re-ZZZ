using System;

using SPFramework;

namespace GamePlay.Team
{
    /// <summary>
    /// 队伍切换请求接受事件
    /// </summary>
    public readonly struct TeamCharacterSwitchedEvent : IEvent
    {
        /// <summary>
        /// 请求接受后的队伍只读快照 不代表退场动画已完成
        /// </summary>
        public TeamReadOnlyInfo TeamInfo { get; }

        /// <summary>
        /// 创建队伍切换请求接受事件
        /// </summary>
        /// <param name="teamInfo">请求接受后的队伍只读快照</param>
        public TeamCharacterSwitchedEvent(TeamReadOnlyInfo teamInfo)
        {
            if (teamInfo == null)
            {
                throw new ArgumentNullException(nameof(teamInfo));
            }

            TeamInfo = teamInfo;
        }
    }
}
