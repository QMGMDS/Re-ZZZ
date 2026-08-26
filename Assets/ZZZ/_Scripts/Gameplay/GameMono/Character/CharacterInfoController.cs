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

        private void Start()
        {
            if (_characterInfoAsset == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterInfoController)} 要求必须分配 {nameof(_characterInfoAsset)}");
            }

            _runtime = new CharacterInfoRuntime(_characterInfoAsset);
        }

        public void DriveCharacter()
        {
            if (_runtime == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterInfoController)} 必须先完成 Start");
            }

            CharacterDriven?.Invoke(_runtime);
        }
    }
}
