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

        /// <summary>
        /// 写入本次逻辑 Tick 的角色运行时数据
        /// </summary>
        public void WriteRuntimeData(InputCharacterData inputCharacterData)
        {
            _runtime.Intention = inputCharacterData.Intention;
            _runtime.MoveDirection = inputCharacterData.MoveInput;
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
