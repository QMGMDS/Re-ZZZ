using System;
using System.Collections.Generic;

using UnityEngine;

using SPFramework;
using GamePlay.Definition;
using GamePlay.Input;
using GamePlay.Input.Public;

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
            if (!ServiceHub.TryGet<IInputService>(out IInputService inputService))
            {
                throw new InvalidOperationException("未得到本帧输入服务接口");
            }

            // 输入写入激活角色
            CharacterInputData characterInputData = inputService.CharacterInputData;
            _currentActiveCharacter.SetIntention(TranslateInput(characterInputData));

            _currentActiveCharacter.CharacterUpdate();
        }

        private static CharacterIntention TranslateInput(CharacterInputData characterInputData)
        {
            return new CharacterIntention(
                ToTrilean(characterInputData.Move.sqrMagnitude != 0f),
                ToTrilean(characterInputData.Attack),
                ToTrilean(characterInputData.Evade),
                ToTrilean(characterInputData.Skill),
                ToTrilean(characterInputData.Ultimate));
        }

        private static Trilean ToTrilean(bool value)
        {
            return value ? Trilean.True : Trilean.False;
        }
    }
}
