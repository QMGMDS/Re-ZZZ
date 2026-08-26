using System.Collections.Generic;

using UnityEngine;

using SPFramework;
using GamePlay.GameModule;
using GamePlay.Data;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 角色攻击检测碰撞体
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CharacterAttackCollider : MonoBehaviour
    {
        [SerializeField, Tooltip("父级实体")]
        private Entity _attackerEntity;

        private readonly HashSet<AttackRequest> _handledRequests = new HashSet<AttackRequest>();

        private Collider _collider;
        private ICombatModule _combatModule;
        private bool _isDetectionWindowOpen;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (!_collider.isTrigger)
            {
                throw new System.InvalidOperationException(
                    $"{nameof(CharacterAttackCollider)} 要求所在 Collider 必须启用 Is Trigger");
            }

            if (_attackerEntity == null)
            {
                throw new System.InvalidOperationException("未配置父级实体");
            }

            _combatModule = ModuleSystem.GetModule<ICombatModule>();
            _collider.enabled = false;
        }

        /// <summary>
        /// 开启攻击检测窗口
        /// </summary>
        public void OpenAttackDetectionWindow()
        {
            _isDetectionWindowOpen = true;
            _collider.enabled = true;
        }

        /// <summary>
        /// 关闭攻击检测窗口
        /// 并清空本窗口的碰撞去重记录
        /// </summary>
        public void CloseAttackDetectionWindow()
        {
            _isDetectionWindowOpen = false;
            _collider.enabled = false;
            _handledRequests.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            TrySubmitAttack(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TrySubmitAttack(other);
        }

        private void TrySubmitAttack(Collider other)
        {
            if (!_isDetectionWindowOpen)
            {
                return;
            }

            Entity targetEntity = other.GetComponentInParent<Entity>();
            if (targetEntity == null)
            {
                return;
            }

            EntityId attackerId = _attackerEntity.Id;
            EntityId targetId = targetEntity.Id;
            if (!attackerId.IsValid || !targetId.IsValid || attackerId == targetId)
            {
                return;
            }

            AttackRequest attackRequest = new AttackRequest(attackerId, targetId);
            if (!_handledRequests.Add(attackRequest))
            {
                return;
            }

            _combatModule.SubmitAttack(attackRequest);
        }

        private void OnDisable()
        {
            _isDetectionWindowOpen = false;
            _collider.enabled = false;
            _handledRequests.Clear();
        }
    }
}
