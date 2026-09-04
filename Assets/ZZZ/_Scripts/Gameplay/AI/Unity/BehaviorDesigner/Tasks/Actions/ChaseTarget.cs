using System;

using BehaviorDesigner.Runtime.Tasks;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

using GamePlay.Character;
using GamePlay.Definition;

namespace GamePlay.AI.BehaviorDesigner.Tasks.Actions
{
    [TaskCategory("ZZZ/AI")]
    [TaskDescription("持续写入追逐当前玩家的意图和方向")]
    public sealed class ChaseTarget : Action
    {
        private static readonly CharacterIntention ChaseIntention = new CharacterIntention(
            Trilean.True,
            Trilean.False,
            Trilean.False,
            Trilean.False,
            Trilean.False);

        private AIBrain _brain;

        public override void OnAwake()
        {
            _brain = Owner.GetComponent<AIBrain>();
            if (_brain == null)
            {
                throw new InvalidOperationException($"{nameof(ChaseTarget)} 的拥有者必须包含 {nameof(AIBrain)}");
            }
        }

        public override TaskStatus OnUpdate()
        {
            _brain.WriteIntention(ChaseIntention);
            _brain.WriteMoveDirectionInWorld(_brain.GetDirectionToPlayerInWorld());
            return TaskStatus.Running;
        }
    }
}
