using System;

using UnityEngine;

using GamePlay.Character.Contract;
using GamePlay.Collider;
using GamePlay.Team.Contract;
using SPFramework;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色动作控制 Root Mono
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterActionController : MonoBehaviour,
        ICharacterUpdateTarget,
        ICharacterHurtReceiver,
        ITeamCharacter
    {
        // 必要组件
        private CharacterController _characterController;
        private Animator _animator;

        [Header("自定义配置")]
        [SerializeField, Tooltip("本角色的信息资产")]
        private CharacterInfoAsset _characterInfoAsset;
        [SerializeField, Tooltip("本角色的动作资产集合")]
        private CharacterActionSetAsset _actionSet;
        [SerializeField, Tooltip("本角色的攻击碰撞体")]
        private ColliderController _attackCollider;

        private ICharacterModule _characterModule;

        private int _entityId;

        public int EntityId => _entityId;

        // 动作逻辑模型
        private CharacterActionModel _actionModel;
        // 位移器
        private CharacterDisplacementDriver _displacementDriver;
        // 旋转器
        private CharacterRotationDriver _rotationDriver;
        // 动画驱动器
        private CharacterAnimationDriver _animationDriver;
        // 攻击机器
        private CharacterAttackMachine _attackMachine;
        private ITeamModule _teamModule;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (_characterInfoAsset == null
                || _actionSet == null
                || _attackCollider == null)
            {
                throw new InvalidOperationException(
                    "检查角色信息资产、动作资产集合、攻击碰撞体配置");
            }

            _actionSet.BuildRuntimeLookups(out var actionsById, out var linksBySourceActionId);

            _actionModel = new CharacterActionModel(
                _characterInfoAsset,
                _actionSet.DefaultAction,
                actionsById,
                linksBySourceActionId);

            _displacementDriver = new CharacterDisplacementDriver(_characterController);
            _rotationDriver = new CharacterRotationDriver(transform);
            _animationDriver = new CharacterAnimationDriver(_animator, linksBySourceActionId);
            _attackMachine = new CharacterAttackMachine(_attackCollider);
        }

        private void OnEnable()
        {
            if (!ServiceHub.TryGet<ICharacterModule>(out ICharacterModule characterModule))
            {
                throw new InvalidOperationException(
                    $"{nameof(ICharacterModule)} 未注册 不能启用 {nameof(CharacterActionController)}");
            }

            _entityId = characterModule.Register(this, this, _actionModel.Runtime);
            _characterModule = characterModule;
        }

        private void OnDisable()
        {
            _attackMachine.CloseIfOpen();
            _characterModule.Unregister(this);
            _characterModule = null;
        }

        private void Start()
        {
            if (!ServiceHub.TryGet<ITeamModule>(out ITeamModule teamModule))
            {
                throw new InvalidOperationException(
                    $"{nameof(ITeamModule)} 未注册 不能注册 {nameof(CharacterActionController)}");
            }

            teamModule.Register(this);
            _teamModule = teamModule;
        }

        private void OnDestroy()
        {
            if (_teamModule != null)
            {
                _teamModule.Unregister(this);
                _teamModule = null;
            }

            if (_animationDriver != null)
            {
                _animationDriver.Dispose();
            }
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            CharacterActionState actionState = _actionModel.LogicUpdate(
                tickDeltaSeconds,
                transform.forward);

            _displacementDriver.Evaluate(actionState);
            _rotationDriver.Evaluate(actionState, tickDeltaSeconds);
            _attackMachine.LogicUpdate(actionState, _entityId);
        }

        /// <inheritdoc/>
        public void RenderUpdate(float deltaTimeSeconds)
        {
            _animationDriver.Evaluate(
                _actionModel.CurrentState,
                deltaTimeSeconds);
        }

        /// <inheritdoc/>
        public void ReceiveHit(int damage)
        {
            _actionModel.ReceiveHit(damage);
        }

        /// <summary>
        /// 接收本次逻辑 Tick 的玩家输入
        /// </summary>
        public void ReceivePlayerInput(InputCharacterData inputCharacterData)
        {
            _actionModel.WriteRuntimeData(inputCharacterData);
        }

        /// <inheritdoc/>
        public void EnterField()
        {
            _actionModel.ResetToDefaultAction();
            _animationDriver.ResetToAction(_actionSet.DefaultAction);
            gameObject.SetActive(true);
            _actionModel.RequestSwitchIn();
        }

        /// <inheritdoc/>
        public void ExitField()
        {
            _actionModel.RequestSwitchOut();
        }

        /// <summary>
        /// 由动画事件触发角色失活
        /// </summary>
        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        public void InitializeInactive()
        {
            Deactivate();
        }
    }
}
