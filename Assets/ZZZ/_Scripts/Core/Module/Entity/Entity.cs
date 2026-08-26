using UnityEngine;

namespace SPFramework
{
    /// <summary>
    /// 为当前物体提供统一的实体身份
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Entity : MonoBehaviour
    {
        private EntityId _entityId;
        private IEntityModule _entityModule;

        public EntityId Id => _entityId;

        private void OnEnable()
        {
            IEntityModule entityModule = ModuleSystem.GetModule<IEntityModule>();
            entityModule.Register(this);
            _entityModule = entityModule;
        }

        private void OnDisable()
        {
            if (_entityModule == null)
            {
                return;
            }

            _entityModule.Unregister(this);
            _entityModule = null;
        }

        internal void DetachModule()
        {
            _entityModule = null;
            _entityId = EntityId.Invalid;
        }

        internal void AssignId(EntityId entityId)
        {
            _entityId = entityId;
        }
    }
}
