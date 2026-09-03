using System;

using UnityEngine;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class EnemyCharacterController : MonoBehaviour
    {
        // 依赖组件
        private Animator _animator;

        // 依赖配置
        [SerializeField]
        private CharacterActionSetAsset _characterActionSetAsset;

        // 依赖逻辑模型
        private EnemyCharacterCoordinator _coordinator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (_characterActionSetAsset == null)
            {
                throw new InvalidOperationException($"{nameof(EnemyCharacterController)} 检查配置");
            }

            _coordinator = new EnemyCharacterCoordinator(_characterActionSetAsset, _animator);
        }

        private void OnDestroy()
        {
            _coordinator.Dispose();
            _coordinator = null;
        }
    }
}
