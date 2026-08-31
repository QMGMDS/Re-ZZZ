using System;

using UnityEngine;

using GamePlay.Character.Contract;
using GamePlay.Team.Contract;
using SPFramework;

namespace GamePlay.Team
{
    /// <summary>
    /// 玩家队伍控制器
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class TeamController : MonoBehaviour, ITeamModule
    {
        [Header("队伍配置")]
        [SerializeField, Tooltip("按切换顺序配置场景中的玩家角色组件")]
        private MonoBehaviour[] _characters;

        private ITeamCharacter[] _teamCharacters;
        private int _currentCharacterIndex;
        private bool _isConfigured;
        private bool _isServiceRegistered;
        private IDisposable _switchRequestSubscription;

        /// <inheritdoc/>
        public ITeamCharacter CurrentCharacter
        {
            get
            {
                EnsureConfigured();
                return _teamCharacters[_currentCharacterIndex];
            }
        }

        private void Awake()
        {
            _teamCharacters = BuildCharacters();
            _currentCharacterIndex = 0;
            _isConfigured = true;

            // _teamCharacters[_currentCharacterIndex].EnterField(_characters[_currentCharacterIndex].transform);

            ServiceHub.Register<ITeamModule>(this);
            _isServiceRegistered = true;
        }

        private void OnEnable()
        {
            _switchRequestSubscription = EventBus.Subscribe<TeamCharacterSwitchRequestedEvent>(OnCharacterSwitchRequested);
        }

        private void OnDisable()
        {
            if (_switchRequestSubscription != null)
            {
                _switchRequestSubscription.Dispose();
                _switchRequestSubscription = null;
            }
        }

        private void OnCharacterSwitchRequested(TeamCharacterSwitchRequestedEvent eventData)
        {
            EnsureConfigured();

            if (!CanSwitch())
            {
                return;
            }

            int targetCharacterIndex = (_currentCharacterIndex + 1) % _teamCharacters.Length;
            ITeamCharacter currentCharacter = _teamCharacters[_currentCharacterIndex];
            ITeamCharacter targetCharacter = _teamCharacters[targetCharacterIndex];

            currentCharacter.ExitField();
            targetCharacter.EnterField(eventData.CharacterTransform);
            _currentCharacterIndex = targetCharacterIndex;

            TeamReadOnlyInfo teamInfo = CreateReadOnlyInfo();

            EventBus.Publish(new TeamCharacterSwitchedEvent(teamInfo, _characters[targetCharacterIndex].transform));
        }

        private bool CanSwitch()
        {
            return _teamCharacters.Length >= 2;
        }

        private ITeamCharacter[] BuildCharacters()
        {
            if (_characters == null || _characters.Length == 0)
            {
                throw new InvalidOperationException($"{nameof(TeamController)} 必须配置至少一个场景角色");
            }

            ITeamCharacter[] characters = new ITeamCharacter[_characters.Length];

            for (int index = 0; index < _characters.Length; index++)
            {
                MonoBehaviour configuredCharacter = _characters[index];

                if (configuredCharacter == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(TeamController)} 的角色列表第 {index} 项未配置");
                }

                if (!(configuredCharacter is ITeamCharacter teamCharacter))
                {
                    throw new InvalidOperationException(
                        $"{nameof(TeamController)} 的角色列表第 {index} 项必须实现 {nameof(ITeamCharacter)}");
                }

                for (int previousIndex = 0; previousIndex < index; previousIndex++)
                {
                    if (ReferenceEquals(characters[previousIndex], teamCharacter))
                    {
                        throw new InvalidOperationException(
                            $"{nameof(TeamController)} 的角色列表第 {index} 项与第 {previousIndex} 项重复");
                    }
                }

                characters[index] = teamCharacter;
            }

            return characters;
        }

        private TeamReadOnlyInfo CreateReadOnlyInfo()
        {
            TeamCharacterReadOnlyInfo[] characters =
                new TeamCharacterReadOnlyInfo[_teamCharacters.Length];

            for (int index = 0; index < _teamCharacters.Length; index++)
            {
                characters[index] = new TeamCharacterReadOnlyInfo(
                    _teamCharacters[index].EntityId,
                    index,
                    index == _currentCharacterIndex);
            }

            return new TeamReadOnlyInfo(characters, _currentCharacterIndex);
        }

        private void EnsureConfigured()
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException(
                    $"{nameof(TeamController)} 尚未初始化");
            }
        }

        private void OnDestroy()
        {
            if (!_isServiceRegistered)
            {
                return;
            }

            ServiceHub.Unregister<ITeamModule>(this);
            _isServiceRegistered = false;
        }
    }
}
