using System;

using SPFramework;

namespace GamePlay.Data
{
    /// <summary>
    /// 一次攻击者与受击者之间的攻击请求
    /// </summary>
    public readonly struct AttackRequest
    {
        public EntityId AttackerId { get; }
        public EntityId TargetId { get; }

        public AttackRequest(EntityId attackerId, EntityId targetId)
        {
            AttackerId = attackerId;
            TargetId = targetId;
        }
    }
}
