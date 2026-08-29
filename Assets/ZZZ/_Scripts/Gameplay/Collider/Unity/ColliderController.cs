using System;

using UnityEngine;

using SPFramework;
using GamePlay.Character;
using GamePlay.Collider.Contract;

namespace GamePlay.Collider
{
    /// <summary>
    /// 碰撞体控制器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ColliderController : MonoBehaviour, IColliderUpdateTarget, ICombatCollider
    {
        // 查询缓冲区的初始容量
        private const int INITIAL_QUERY_BUFFER_CAPACITY = 16;
        // 无效角色实体 ID
        private const int INVALID_ENTITY_ID = -1;

        private UnityEngine.Collider _collider;
        private UnityEngine.Collider[] _overlapResults;

        private IColliderModule _colliderModule;
        private int _ownerEntityId = INVALID_ENTITY_ID;
        private bool _isAttackColliderOpen;

        private void Awake()
        {
            UnityEngine.Collider[] colliders = GetComponents<UnityEngine.Collider>();

            if (colliders.Length != 1)
            {
                throw new InvalidOperationException(
                    "ColliderController 要求同一个 GameObject 上存在且仅存在一个 Collider");
            }

            _collider = colliders[0];

            if (!IsSupportedAsPenetrationSource(_collider))
            {
                throw new InvalidOperationException(
                    "ColliderController 的 Collider 必须是 BoxCollider SphereCollider CapsuleCollider 或凸 MeshCollider");
            }

            _overlapResults = new UnityEngine.Collider[INITIAL_QUERY_BUFFER_CAPACITY];
            _collider.enabled = false;
        }

        private static bool IsSupportedAsPenetrationSource(UnityEngine.Collider collider)
        {
            if (collider is BoxCollider
                || collider is SphereCollider
                || collider is CapsuleCollider)
            {
                return true;
            }

            return collider is MeshCollider meshCollider && meshCollider.convex;
        }

        /// <inheritdoc/>
        public void OpenAttackCollider(int entityId)
        {
            if (_isAttackColliderOpen)
            {
                throw new InvalidOperationException("攻击碰撞体已经打开");
            }

            if (!ServiceHub.TryGet<IColliderModule>(out IColliderModule colliderModule))
            {
                throw new InvalidOperationException(
                    $"{nameof(IColliderModule)} 未注册 不能打开 {nameof(ColliderController)}");
            }

            colliderModule.Register(this);
            _colliderModule = colliderModule;
            _ownerEntityId = entityId;
            _isAttackColliderOpen = true;
            _collider.enabled = true;
        }

        /// <inheritdoc/>
        public void CloseAttackCollider()
        {
            if (!_isAttackColliderOpen)
            {
                throw new InvalidOperationException("攻击碰撞体尚未打开");
            }

            _collider.enabled = false;
            _colliderModule.Unregister(this);
            _colliderModule = null;
            _ownerEntityId = INVALID_ENTITY_ID;
            _isAttackColliderOpen = false;
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            ProcessCollisions();
        }

        private void ProcessCollisions()
        {
            Bounds bounds = _collider.bounds;
            int resultCount = QueryCandidates(bounds);

            for (int index = 0; index < resultCount; index++)
            {
                UnityEngine.Collider candidate = _overlapResults[index];

                // 自身碰撞体或 Trigger 排除
                if (candidate == _collider || candidate.isTrigger)
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                    _collider,
                    _collider.transform.position,
                    _collider.transform.rotation,
                    candidate,
                    candidate.transform.position,
                    candidate.transform.rotation,
                    out _,
                    out _))
                {
                    CharacterActionController target =
                        candidate.GetComponentInParent<CharacterActionController>();

                    if (target == null)
                    {
                        continue;
                    }

                    Debug.Log(
                        $"攻击方实体 ID {_ownerEntityId} 被碰撞角色实体 ID {target.EntityId}",
                        this);
                }
            }
        }

        private int QueryCandidates(Bounds bounds)
        {
            while (true)
            {
                int resultCount = Physics.OverlapBoxNonAlloc(
                    bounds.center,
                    bounds.extents,
                    _overlapResults,
                    Quaternion.identity,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore);

                if (resultCount < _overlapResults.Length)
                {
                    return resultCount;
                }

                Array.Resize(ref _overlapResults, _overlapResults.Length * 2);
            }
        }
    }
}
