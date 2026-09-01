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
    }
}
