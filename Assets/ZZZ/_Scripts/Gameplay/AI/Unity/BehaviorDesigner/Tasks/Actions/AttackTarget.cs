using System;

using BehaviorDesigner.Runtime.Tasks;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

using GamePlay.Character;
using GamePlay.Definition;

namespace GamePlay.AI.BehaviorDesigner.Tasks.Actions
{
    [TaskCategory("ZZZ/AI")]
    [TaskDescription("持续写入攻击当前玩家的意图")]
    public sealed class AttackTarget : Action
    {
        private static readonly CharacterIntention AttackIntention = new CharacterIntention(
            Trilean.False,
            Trilean.True,
            Trilean.False,
            Trilean.False,
            Trilean.False);

        private AIBrain _brain;

        public override void OnAwake()
        {
            _brain = Owner.GetComponent<AIBrain>();
            if (_brain == null)
            {
                throw new InvalidOperationException($"{nameof(AttackTarget)} 的拥有者必须包含 {nameof(AIBrain)}");
            }
        }

        public override TaskStatus OnUpdate()
        {
            _brain.WriteIntention(AttackIntention);
            return TaskStatus.Running;
        }
    }
}
