using GamePlay.Character;

namespace GamePlay.Team.Contract
{
    /// <summary>
    /// 玩家输入接收者契约
    /// </summary>
    public interface IPlayerInputReceiver
    {
        /// <summary>
        /// 接收一次逻辑 Tick 的玩家输入
        /// </summary>
        /// <param name="inputCharacterData">本次逻辑 Tick 的角色输入</param>
        void ReceivePlayerInput(InputCharacterData inputCharacterData);
    }
}
