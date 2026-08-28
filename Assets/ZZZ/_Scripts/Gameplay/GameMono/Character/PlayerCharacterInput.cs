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
    public sealed class PlayerCharacterInput : MonoBehaviour, IPlayerCharacterInput
    {
        private CharacterInfoController _characterInfoController;
        private IIputData _inputData;
        private ICameraService _cameraService;
        private bool _isInitialized;

        private void Awake()
        {
            _characterInfoController = GetComponent<CharacterInfoController>();
            if (_characterInfoController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerCharacterInput)} 要求所在物体必须包含 {nameof(CharacterInfoController)}");
            }
        }

        private void OnEnable()
        {
            ServiceHub.Register<IPlayerCharacterInput>(this);

            if (_isInitialized)
            {
                BindServices();
            }
        }

        private void Start()
        {
            BindServices();
            _isInitialized = true;
        }

        private void BindServices()
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

        /// <inheritdoc/>
        public void WriteRuntimeData(float logicalTimeSeconds)
        {
            CharacterInputData characterInputData = _inputData.ConsumeCharacterInput();
            Vector2 worldMoveDirection = _cameraService.ConvertToWorldCoordinate(
                characterInputData.Move);

            CharacterIntention intention = new CharacterIntention(
                worldMoveDirection.sqrMagnitude == 0f ? Trilean.False : Trilean.True,
                characterInputData.Attack ? Trilean.True : Trilean.False,
                characterInputData.Evade ? Trilean.True : Trilean.False,
                characterInputData.Skill ? Trilean.True : Trilean.False,
                characterInputData.Ultimate ? Trilean.True : Trilean.False,
                characterInputData.Switch ? Trilean.True : Trilean.False);

            InputCharacterData inputCharacterData = new InputCharacterData(
                logicalTimeSeconds,
                intention,
                worldMoveDirection);

            _characterInfoController.WriteRuntimeData(inputCharacterData);
        }

        private void OnDisable()
        {
            ServiceHub.Unregister<IPlayerCharacterInput>(this);
        }
    }
}
