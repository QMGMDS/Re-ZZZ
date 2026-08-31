using SPFramework;

namespace GamePlay.Team.Contract
{
    /// <summary>
    /// 队伍模块契约
    /// </summary>
    public interface ITeamModule : IService
    {
        /// <summary>
        /// 当前逻辑角色
        /// </summary>
        ITeamCharacter CurrentCharacter { get; }

        /// <summary>
        /// 请求切换到队伍中的下一个角色
        /// </summary>
        /// <param name="requester">发起请求的当前角色</param>
        /// <returns>是否接受切换请求</returns>
        bool TryRequestSwitch(ITeamCharacter requester);
    }
}
