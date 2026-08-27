using System;

using UnityEngine;

using GamePlay.Data;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 角色信息控制器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterInfoController : MonoBehaviour
    {
        [Header("自定义配置")]
        [SerializeField, Tooltip("本角色的信息资产")]
        private CharacterInfoAsset _characterInfoAsset;

        private CharacterInfoRuntime _runtime;

        public CharacterInfoRuntime Runtime => _runtime;

        public event Action<CharacterInfoRuntime> CharacterDriven;
        public event Action<AttackRequest> AttackReceived;

        private void Awake()
        {
            if (_characterInfoAsset == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterInfoController)} 要求必须分配 {nameof(_characterInfoAsset)}");
            }

            _runtime = new CharacterInfoRuntime(_characterInfoAsset);
        }

        public void PlayerChange(Vector2 moveDirection, bool attack, bool evade)
        {
            _runtime.Intention = new CharacterIntention(
                moveDirection.sqrMagnitude == 0f ? Trilean.False : Trilean.True,
                attack ? Trilean.True : Trilean.False,
                evade ? Trilean.True : Trilean.False);

            _runtime.MoveDirection = moveDirection;

            DriveCharacter();
        }

        private void DriveCharacter()
        {
            CharacterDriven?.Invoke(_runtime);
        }

        /// <summary>
        /// 角色受击入口
        /// </summary>
        public void ReceiveAttack(AttackRequest attackRequest)
        {
            AttackReceived?.Invoke(attackRequest);
        }
    }
}
