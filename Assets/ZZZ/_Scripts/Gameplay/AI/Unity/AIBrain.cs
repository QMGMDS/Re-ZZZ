using System;

using UnityEngine;

using BehaviorDesigner.Runtime;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

using GamePlay.Character;
using GamePlay.Character.Public;
using SPFramework;

namespace GamePlay.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCharacterController))]
    public sealed class AIBrain : MonoBehaviour
    {
        // 依赖配置
        [SerializeField]
        private Behavior _behaviorTree;
        [SerializeField, Tooltip("敌人 AI 静态配置资产")]
        private AIConfigAsset _configAsset;

        // 依赖逻辑模型
        private EnemyCharacterController _enemyCharacterController;

        // 运行时感知数据
        private AIPerceptionData _perceptionData;

        private IPlayerTeamService _playerTeamService;
        private BehaviorManager _behaviorManager;

        private void Awake()
        {
            _enemyCharacterController = GetComponent<EnemyCharacterController>();

            if (_behaviorTree == null || _configAsset == null)
            {
                throw new InvalidOperationException($"{nameof(AIBrain)} 检查配置");
            }

            if (_behaviorTree.ExternalBehavior == null)
            {
                throw new InvalidOperationException($"{nameof(AIBrain)} 的 Behavior Tree 必须配置外部行为树");
            }

            if (!_behaviorTree.StartWhenEnabled)
            {
                throw new InvalidOperationException($"{nameof(AIBrain)} 的 Behavior Tree 必须启用 Start When Enabled");
            }

            _perceptionData = new AIPerceptionData();
        }

        private void Start()
        {
            if (!ServiceHub.TryGet<IPlayerTeamService>(out IPlayerTeamService playerTeamService))
            {
                throw new InvalidOperationException($"{nameof(AIBrain)} 未获取到 {nameof(IPlayerTeamService)}");
            }
            _playerTeamService = playerTeamService;

            _behaviorManager = BehaviorManager.instance;

            if (_behaviorManager.UpdateInterval != UpdateIntervalType.Manual)
            {
                throw new InvalidOperationException($"{nameof(BehaviorManager)} 的 Update Interval 必须配置为 Manual");
            }
        }

        private void Update()
        {
            Transform playerTransform = _playerTeamService.CurrentActiveCharacterTransform;
            _perceptionData.SetPlayerPositionInWorld(playerTransform.position);

            _behaviorManager.Tick(_behaviorTree);
            _enemyCharacterController.CharacterUpdate();
        }

        private bool IsPlayerInSector(float radius, float angleDegrees)
        {
            Vector3 direction = _perceptionData.PlayerPositionInWorld - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > radius * radius)
            {
                return false;
            }

            if (direction.sqrMagnitude == 0f)
            {
                return true;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            return Vector3.Angle(forward, direction) <= angleDegrees * 0.5f;
        }

        #region 供节点使用

        /// <summary>
        /// 是否在视野范围内
        /// </summary>
        public bool IsPlayerInVisionRange()
        {
            return IsPlayerInSector(_configAsset.VisionRadius, _configAsset.VisionAngleDegrees);
        }

        /// <summary>
        /// 是否在攻击范围内
        /// </summary>
        public bool IsPlayerInAttackRange()
        {
            return IsPlayerInSector(_configAsset.AttackRadius, _configAsset.AttackAngleDegrees);
        }

        /// <summary>
        /// 获取玩家方位（世界空间方位）
        /// </summary>
        public Vector2 GetDirectionToPlayerInWorld()
        {
            Vector3 direction = _perceptionData.PlayerPositionInWorld - transform.position;
            var directionInWorld = new Vector2(direction.x, direction.z);
            return directionInWorld.sqrMagnitude == 0f ? Vector2.zero : directionInWorld.normalized;
        }

        /// <summary>
        /// 写入意图
        /// </summary>
        public void WriteIntention(CharacterIntention intention)
        {
            _enemyCharacterController.SetIntention(intention);
        }

        /// <summary>
        /// 写入移动方位
        /// </summary>
        public void WriteMoveDirectionInWorld(Vector2 moveDirectionInWorld)
        {
            _enemyCharacterController.SetMoveDirectionInWorld(moveDirectionInWorld);
        }

        #endregion
    }
}
