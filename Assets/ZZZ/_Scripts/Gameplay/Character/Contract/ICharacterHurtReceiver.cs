namespace GamePlay.Character.Contract
{
    /// <summary>
    /// 角色受击入口契约
    /// </summary>
    public interface ICharacterHurtReceiver
    {
        /// <summary>
        /// 接收一次角色受击
        /// </summary>
        /// <param name="damage">本次受击伤害值</param>
        void ReceiveHit(int damage);
    }
}
