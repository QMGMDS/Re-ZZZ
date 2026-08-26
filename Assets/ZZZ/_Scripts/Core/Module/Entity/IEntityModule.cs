using System;
using System.Collections.Generic;

namespace SPFramework
{
    /// <summary>
    /// 管理当前游戏中所有激活实体
    /// </summary>
    public interface IEntityModule
    {
        event Action<Entity> EntityRegistered;

        event Action<Entity> EntityUnregistered;

        IReadOnlyCollection<Entity> ActiveEntities { get; }

        void Register(Entity entity);

        void Unregister(Entity entity);

        bool TryGetEntity(EntityId entityId, out Entity entity);
    }
}
