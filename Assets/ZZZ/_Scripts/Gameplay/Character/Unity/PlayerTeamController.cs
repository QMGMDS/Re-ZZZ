using System;
using System.Collections.Generic;

using UnityEngine;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerTeamController : MonoBehaviour
    {
        [SerializeField, Tooltip("队伍列表，默认第一位为初始激活角色")]
        private List<PlayerCharacterController> _playerCharacterControllers = new List<PlayerCharacterController>();

        // 队伍中当前激活角色
        private PlayerCharacterController _currentActiveCharacter;

        private void Awake()
        {
            if (_playerCharacterControllers == null || _playerCharacterControllers.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(PlayerTeamController)} 必须配置 {nameof(_playerCharacterControllers)}");
            }

            _currentActiveCharacter = _playerCharacterControllers[0];
        }

        private void Update()
        {
            _currentActiveCharacter.CharacterUpdate();
        }
    }
}
