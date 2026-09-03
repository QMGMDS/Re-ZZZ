using System;
using System.Collections.Generic;

using UnityEngine;

using SPFramework;
using GamePlay.Definition;
using GamePlay.Input;
using GamePlay.Input.Public;
using GamePlay.Camera.Public;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerTeamController : MonoBehaviour
    {
        [SerializeField, Tooltip("队伍列表，默认第一位为初始激活角色")]
        private List<PlayerCharacterController> _playerCharacterControllers = new List<PlayerCharacterController>();

        // 队伍中当前激活角色
        private PlayerCharacterController _currentActiveCharacter;
        private int _currentActiveCharacterIndex;

        private void Awake()
        {
            if (_playerCharacterControllers == null || _playerCharacterControllers.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(PlayerTeamController)} 必须配置 {nameof(_playerCharacterControllers)}");
            }

            for (int index = 0; index < _playerCharacterControllers.Count; index++)
            {
                PlayerCharacterController playerCharacterController = _playerCharacterControllers[index];
                playerCharacterController.InitializeCharacterInfo(index);
            }

            _currentActiveCharacterIndex = 0;
            _currentActiveCharacter = _playerCharacterControllers[_currentActiveCharacterIndex];
        }

        private void Update()
        {
            if (!ServiceHub.TryGet<IInputService>(out IInputService inputService))
            {
                throw new InvalidOperationException("未得到本帧输入服务接口");
            }

            if (!ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                throw new InvalidOperationException("未得到本帧摄像机服务接口");
            }

            CharacterInputData characterInputData = inputService.CharacterInputData;

            // 角色切换判断
            if (characterInputData.Switch)
            {
                SwitchToNextCharacter();
            }

            // 输入写入激活角色
            Vector2 moveDirectionInWorld = cameraService.ConvertToWorldCoordinate(characterInputData.Move);
            _currentActiveCharacter.SetIntention(TranslateInput(characterInputData), moveDirectionInWorld);

            // 遍历角色更新
            for (int index = 0; index < _playerCharacterControllers.Count; index++)
            {
                PlayerCharacterController playerCharacterController = _playerCharacterControllers[index];
                playerCharacterController.CharacterUpdate();
            }
        }

        /// <summary>
        /// 检测输入 Switch - 角色切换
        /// </summary>
        private void SwitchToNextCharacter()
        {
            int nextCharacterIndex = (_currentActiveCharacterIndex + 1) % _playerCharacterControllers.Count;
            PlayerCharacterController nextCharacter = _playerCharacterControllers[nextCharacterIndex];

            _currentActiveCharacter.ExitField();
            nextCharacter.EnterField();

            _currentActiveCharacterIndex = nextCharacterIndex;
            _currentActiveCharacter = nextCharacter;
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
