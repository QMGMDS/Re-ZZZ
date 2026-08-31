using GamePlay.Character;
using GamePlay.Character.Contract;

namespace GamePlay.Team.Contract
{
    /// <summary>
    /// 队伍角色契约
    /// </summary>
    public interface ITeamCharacter : ICharacterUpdateTarget
    {
        /// <summary>
        /// 接收本次逻辑 Tick 的玩家输入
        /// </summary>
        void ReceivePlayerInput(InputCharacterData inputCharacterData);

        /// <summary>
        /// 角色上场
        /// </summary>
        void EnterField();
    }
}
