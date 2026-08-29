using SPFramework;

namespace GamePlay.Team.Contract
{
    /// <summary>
    /// 队伍模块契约
    /// </summary>
    public interface ITeamModule : IService
    {
        /// <summary>
        /// 注册一个角色进入队伍
        /// </summary>
        /// <param name="character">申请进入队伍的角色</param>
        void Register(ITeamCharacter character);

        /// <summary>
        /// 将一个角色从队伍中注销
        /// </summary>
        /// <param name="character">申请离开队伍的角色</param>
        void Unregister(ITeamCharacter character);
    }
}
