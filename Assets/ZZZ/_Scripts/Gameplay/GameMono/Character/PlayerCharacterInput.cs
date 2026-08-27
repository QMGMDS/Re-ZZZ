using System;

using UnityEngine;

using GamePlay.Contract;
using GamePlay.Data;
using SPFramework;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 玩家角色输入
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterInput : MonoBehaviour
    {
        private CharacterInfoController _characterInfoController;
        private IIputData _inputData;
        private ICameraService _cameraService;

        private void Awake()
        {
            _characterInfoController = GetComponent<CharacterInfoController>();
        }

        private void Start()
        {
            if (!ServiceHub.TryGet<IIputData>(out _inputData))
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterInput)} 要求已注册 {nameof(IIputData)}");
            }

            if (!ServiceHub.TryGet<ICameraService>(out _cameraService))
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterInput)} 要求已注册 {nameof(ICameraService)}");
            }
        }

        private void Update()
        {
            CharacterInputData characterInputData = _inputData.CharacterInputData;
            Vector2 worldMoveDirection = _cameraService.ConvertToWorldCoordinate(
                characterInputData.Move);

            _characterInfoController.PlayerChange(
                worldMoveDirection,
                characterInputData.Attack,
                characterInputData.Evade,
                characterInputData.Skill,
                characterInputData.Ultimate);
        }
    }
}
