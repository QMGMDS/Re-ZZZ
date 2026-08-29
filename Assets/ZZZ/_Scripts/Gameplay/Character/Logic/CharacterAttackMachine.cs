using System;

using GamePlay.Collider.Contract;

namespace GamePlay.Character
{
    /// <summary>
    /// 根据当前动作攻击时间控制攻击碰撞体
    /// </summary>
    public sealed class CharacterAttackMachine
    {
        private readonly ICombatCollider _attackCollider;

        private CharacterActionAsset _lastAction;
        private bool _isAttackColliderOpen;

        public CharacterAttackMachine(ICombatCollider attackCollider)
        {
            _attackCollider = attackCollider;
        }

        /// <summary>
        /// 根据当前动作和逻辑时间更新攻击碰撞体状态
        /// </summary>
        public void LogicUpdate(CharacterActionState actionState, int entityId)
        {
            CharacterActionAsset currentAction = actionState.CurrentAction;
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            float logicalProgressSeconds = actionState.LogicalProgressSeconds;
            if (_lastAction != currentAction)
            {
                CloseIfOpen();
            }

            if (currentAction.IsAttackActiveAt(logicalProgressSeconds))
            {
                if (!_isAttackColliderOpen)
                {
                    _attackCollider.OpenAttackCollider(entityId);
                    _isAttackColliderOpen = true;
                }
            }
            else
            {
                CloseIfOpen();
            }

            _lastAction = currentAction;
        }

        /// <summary>
        /// 关闭当前已开启的攻击碰撞体
        /// </summary>
        public void CloseIfOpen()
        {
            if (!_isAttackColliderOpen)
            {
                return;
            }

            _attackCollider.CloseAttackCollider();
            _isAttackColliderOpen = false;
        }
    }
}
