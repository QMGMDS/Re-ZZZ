using System;
using System.Collections.Generic;

using UnityEngine;

using SPFramework;
using GamePlay.Character.Public;
using GamePlay.Definition;
using GamePlay.Input;
using GamePlay.Input.Public;
using GamePlay.Camera.Public;

namespace GamePlay.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerTeamController : MonoBehaviour, IPlayerTeamService
    {
        [SerializeField, Tooltip("队伍列表，默认第一位为初始激活角色")]
        private List<PlayerCharacterController> _playerCharacterControllers = new List<PlayerCharacterController>();

        // 队伍信息列表
        private readonly List<CharacterInfo> _characterInfos = new List<CharacterInfo>();

        // 队伍中当前激活角色索引
        private int _currentActiveCharacterIndex;

        // 借用服务接口
        private IInputService _inputService;
        private ICameraService _cameraService;

        private void Awake()
        {
            if (_playerCharacterControllers == null || _playerCharacterControllers.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(PlayerTeamController)} 必须配置 {nameof(_playerCharacterControllers)}");
            }
            for (int index = 0; index < _playerCharacterControllers.Count; index++)
            {
                PlayerCharacterController playerCharacterController = _playerCharacterControllers[index];
                if (playerCharacterController == null)
                {
                    throw new InvalidOperationException($"{nameof(PlayerTeamController)} 的角色列表第 {index} 项未配置");
                }

                playerCharacterController.RegisterCharacterInfo();
                _characterInfos.Add(playerCharacterController.CharacterInfo);
            }

            _currentActiveCharacterIndex = 0;
            _playerCharacterControllers[_currentActiveCharacterIndex].ActivateInitial();
        }

        private void OnEnable()
        {
            ServiceHub.Register<IPlayerTeamService>(this);
        }

        private void OnDisable()
        {
            ServiceHub.Unregister<IPlayerTeamService>(this);
        }

        private void Start()
        {
            if (!ServiceHub.TryGet<IInputService>(out IInputService inputService))
            {
                throw new InvalidOperationException("未得到本帧输入服务接口");
            }
            _inputService = inputService;
            if (!ServiceHub.TryGet<ICameraService>(out ICameraService cameraService))
            {
                throw new InvalidOperationException("未得到本帧摄像机服务接口");
            }
            _cameraService = cameraService;
        }

        private void Update()
        {
            CharacterInputData characterInputData = _inputService.CharacterInputData;

            // 角色切换判断
            if (characterInputData.Switch && _playerCharacterControllers.Count >= 2)
            {
                SwitchToNextCharacter();
            }

            // 输入写入激活角色
            Vector2 moveDirectionInWorld = _cameraService.ConvertToWorldCoordinate(characterInputData.Move);
            _playerCharacterControllers[_currentActiveCharacterIndex].SetIntention(TranslateInput(characterInputData), moveDirectionInWorld);

            // 遍历角色更新
            for (int index = 0; index < _playerCharacterControllers.Count; index++)
            {
                PlayerCharacterController playerCharacterController = _playerCharacterControllers[index];
                if (!playerCharacterController.isActiveAndEnabled)
                {
                    continue;
                }

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

            Transform currentCharacterTransform = _playerCharacterControllers[_currentActiveCharacterIndex].ExitField();
            nextCharacter.EnterField(currentCharacterTransform);

            _currentActiveCharacterIndex = nextCharacterIndex;

            EventBus.Publish(new CharacterSwitchedEvent(nextCharacter.transform));
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

        #region 服务接口

        /// <inheritdoc/>
        public IReadOnlyList<CharacterInfo> CharacterInfos => _characterInfos;

        /// <inheritdoc/>
        public int CurrentActiveCharacterIndex => _currentActiveCharacterIndex;

        /// <inheritdoc/>
        public Transform CurrentActiveCharacterTransform => _playerCharacterControllers[_currentActiveCharacterIndex].transform;

        #endregion
    }
}
