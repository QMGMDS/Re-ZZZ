using System;

using BehaviorDesigner.Runtime.Tasks;

namespace GamePlay.AI.BehaviorDesigner.Tasks.Conditionals
{
    [TaskCategory("ZZZ/AI")]
    [TaskDescription("判断当前玩家是否位于攻击范围")]
    public sealed class IsAttackRange : Conditional
    {
        private AIBrain _brain;

        public override void OnAwake()
        {
            _brain = Owner.GetComponent<AIBrain>();
            if (_brain == null)
            {
                throw new InvalidOperationException($"{nameof(IsAttackRange)} 的拥有者必须包含 {nameof(AIBrain)}");
            }
        }

        public override TaskStatus OnUpdate()
        {
            return _brain.IsPlayerInAttackRange() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
