namespace GamePlay.Team.Contract
{
    /// <summary>
    /// 队伍角色契约
    /// </summary>
    public interface ITeamCharacter : IPlayerInputReceiver
    {
        /// <summary>
        /// 角色上场
        /// </summary>
        void EnterField();

        /// <summary>
        /// 角色退场
        /// </summary>
        void ExitField();

        /// <summary>
        /// 初始化角色为未上场状态
        /// </summary>
        void InitializeInactive();
    }
}
