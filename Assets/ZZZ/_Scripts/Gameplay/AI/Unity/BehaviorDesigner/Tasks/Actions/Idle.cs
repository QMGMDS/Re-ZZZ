using System;

using UnityEngine;

using BehaviorDesigner.Runtime.Tasks;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

using GamePlay.Character;

namespace GamePlay.AI.BehaviorDesigner.Tasks.Actions
{
    [TaskCategory("ZZZ/AI")]
    [TaskDescription("清空当前角色意图和移动方向")]
    public sealed class Idle : Action
    {
        private AIBrain _brain;

        public override void OnAwake()
        {
            _brain = Owner.GetComponent<AIBrain>();
            if (_brain == null)
            {
                throw new InvalidOperationException($"{nameof(Idle)} 的拥有者必须包含 {nameof(AIBrain)}");
            }
        }

        public override TaskStatus OnUpdate()
        {
            _brain.WriteIntention(CharacterIntention.AllFalse);
            _brain.WriteMoveDirectionInWorld(Vector2.zero);
            return TaskStatus.Running;
        }
    }
}
