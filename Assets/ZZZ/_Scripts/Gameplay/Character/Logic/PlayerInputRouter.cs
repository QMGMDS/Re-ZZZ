using System;

using UnityEngine;

using GamePlay.Camera.Contract;
using GamePlay.Input;
using GamePlay.Input.Contract;

namespace GamePlay.Character
{
    /// <summary>
    /// 玩家输入翻译器
    /// </summary>
    public sealed class PlayerInputRouter
    {
        private readonly IIputData _inputData;
        private readonly ICameraService _cameraService;

        public PlayerInputRouter(IIputData inputData, ICameraService cameraService)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException(nameof(inputData));
            }

            if (cameraService == null)
            {
                throw new ArgumentNullException(nameof(cameraService));
            }

            _inputData = inputData;
            _cameraService = cameraService;
        }

        /// <inheritdoc/>
        public InputCharacterData ConsumePlayerInput(float logicalTimeSeconds)
        {
            CharacterInputData characterInputData = _inputData.ConsumeCharacterInput();
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
