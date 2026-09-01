using System;

using UnityEngine;

using GamePlay.Camera.Contract;
using GamePlay.Input;
using GamePlay.Input.Public;

namespace GamePlay.Character
{
    /// <summary>
    /// 玩家输入翻译器
    /// </summary>
    public sealed class PlayerInputRouter
    {
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;

        public PlayerInputRouter(IInputService inputService, ICameraService cameraService)
        {
            if (inputService == null)
            {
                throw new ArgumentNullException(nameof(inputService));
            }

            if (cameraService == null)
            {
                throw new ArgumentNullException(nameof(cameraService));
            }

            _inputService = inputService;
            _cameraService = cameraService;
        }

        /// <inheritdoc/>
        public InputCharacterData BuildPlayerInput(float logicalTimeSeconds)
        {
            CharacterInputData characterInputData = _inputService.CharacterInputData;
            Vector2 worldMove = _cameraService.ConvertToWorldCoordinate(characterInputData.Move);
            CharacterIntention intention = new CharacterIntention(
                ToTrilean(worldMove.sqrMagnitude > 0f),
                ToTrilean(characterInputData.Attack),
                ToTrilean(characterInputData.Evade),
                ToTrilean(characterInputData.Skill),
                ToTrilean(characterInputData.Ultimate));

            return new InputCharacterData(
                logicalTimeSeconds,
                intention,
                worldMove,
                characterInputData.Switch);
        }

        private static Trilean ToTrilean(bool value)
        {
            return value ? Trilean.True : Trilean.False;
        }
    }
}
