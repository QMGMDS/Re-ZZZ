using System;
using System.Collections.Generic;

namespace SPFramework
{
    /// <summary>
    /// 分配实体 ID 并索引当前激活实体
    /// </summary>
    public sealed class EntityModule : Module, IEntityModule
    {
        private readonly Dictionary<EntityId, Entity> _entities = new Dictionary<EntityId, Entity>();

        private ulong _nextEntityId = 1;

        /// <inheritdoc/>
        public override void OnCreate()
        {
        }

        /// <inheritdoc/>
        public override void OnDestroy()
        {
            foreach (Entity entity in _entities.Values)
            {
                entity.DetachModule();
            }

            _entities.Clear();
            EntityRegistered = null;
            EntityUnregistered = null;
        }

        #region IEntityModule

        /// <inheritdoc/>
        public event Action<Entity> EntityRegistered;

        /// <inheritdoc/>
        public event Action<Entity> EntityUnregistered;

        /// <inheritdoc/>
        public IReadOnlyCollection<Entity> ActiveEntities => _entities.Values;

        /// <inheritdoc/>
        public void Register(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            EntityId entityId = entity.Id.IsValid ? entity.Id : AllocateEntityId();
            if (!_entities.TryAdd(entityId, entity))
            {
                throw new InvalidOperationException($"实体 ID {entityId} 已被注册");
            }

            entity.AssignId(entityId);
            EntityRegistered?.Invoke(entity);
        }

        /// <inheritdoc/>
        public void Unregister(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!_entities.TryGetValue(entity.Id, out Entity registeredEntity)
                || !ReferenceEquals(registeredEntity, entity))
            {
                throw new InvalidOperationException($"实体 ID {entity.Id} 的注销对象与注册对象不一致");
            }

            _entities.Remove(entity.Id);
            EntityUnregistered?.Invoke(entity);
        }

        /// <inheritdoc/>
        public bool TryGetEntity(EntityId entityId, out Entity entity)
        {
            return _entities.TryGetValue(entityId, out entity);
        }

        private EntityId AllocateEntityId()
        {
            if (_nextEntityId == 0)
            {
                throw new InvalidOperationException("实体 ID 已耗尽");
            }

            return new EntityId(_nextEntityId++);
        }

        #endregion
    }
}
