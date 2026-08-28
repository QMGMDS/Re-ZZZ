using System;

using UnityEngine;

using GamePlay.Camera.Contract;
using GamePlay.Character.Contract;
using GamePlay.Input;
using GamePlay.Input.Contract;
using SPFramework;

namespace GamePlay.Character
{
    /// <summary>
    /// 玩家输入调度器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputRouter : MonoBehaviour, IPlayerInputRouter
    {
        [Header("必要组件")]
        [SerializeField, Tooltip("需要写入输入数据的角色信息控制器")]
        private CharacterInfoController _characterInfoController;

        private void Awake()
        {
            if (_characterInfoController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerInputRouter)} 要求必须分配 {nameof(_characterInfoController)}");
            }
        }

        private void OnEnable()
        {
            ServiceHub.Register<IPlayerInputRouter>(this);
        }

        private void OnDisable()
        {
            ServiceHub.Unregister<IPlayerInputRouter>(this);
        }

        /// <inheritdoc/>
        public void LogicUpdate(float logicalTimeSeconds)
        {
            if (!ServiceHub.TryGet<IIputData>(out IIputData inputData)
                || !ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                throw new InvalidOperationException(
                    $"{nameof(IIputData)} 和 {nameof(ICameraService)} 必须在 {nameof(PlayerInputRouter)} 驱动前注册");
            }

            CharacterInputData characterInputData = inputData.ConsumeCharacterInput();
            Vector2 worldMove = cameraService.ConvertToWorldCoordinate(characterInputData.Move);
            CharacterIntention intention = new CharacterIntention(
                ToTrilean(worldMove.sqrMagnitude > 0f),
                ToTrilean(characterInputData.Attack),
                ToTrilean(characterInputData.Evade),
                ToTrilean(characterInputData.Skill),
                ToTrilean(characterInputData.Ultimate),
                ToTrilean(characterInputData.Switch));

            _characterInfoController.WriteRuntimeData(
                new InputCharacterData(logicalTimeSeconds, intention, worldMove));
        }

        private static Trilean ToTrilean(bool value)
        {
            return value ? Trilean.True : Trilean.False;
        }


    }
}
