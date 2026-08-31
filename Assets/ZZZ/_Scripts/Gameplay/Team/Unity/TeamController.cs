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

        private void Awake()
        {
            _teamCharacters = BuildCharacters();
            _currentCharacterIndex = 0;
            _isConfigured = true;

            _switchRequestSubscription =
                EventBus.Subscribe<TeamCharacterSwitchRequestedEvent>(OnCharacterSwitchRequested);

            _teamCharacters[_currentCharacterIndex].EnterField();

            ServiceHub.Register<ITeamModule>(this);
            _isServiceRegistered = true;
        }

        /// <inheritdoc/>
        public ITeamCharacter CurrentCharacter
        {
            get
            {
                EnsureConfigured();
                return _teamCharacters[_currentCharacterIndex];
            }
        }

        private void OnCharacterSwitchRequested(TeamCharacterSwitchRequestedEvent eventData)
        {
            EnsureConfigured();

            if (!CanSwitch(eventData.CharacterTransform))
            {
                return;
            }

            int targetCharacterIndex =
                (_currentCharacterIndex + 1) % _teamCharacters.Length;
            ITeamCharacter currentCharacter = _teamCharacters[_currentCharacterIndex];
            ITeamCharacter targetCharacter = _teamCharacters[targetCharacterIndex];

            currentCharacter.ExitField();
            targetCharacter.EnterField();
            _currentCharacterIndex = targetCharacterIndex;

            TeamReadOnlyInfo teamInfo = CreateReadOnlyInfo();

            EventBus.Publish(new TeamCharacterSwitchedEvent(teamInfo));
        }

        private bool CanSwitch(Transform characterTransform)
        {
            return _teamCharacters.Length >= 2
                && characterTransform == _characters[_currentCharacterIndex].transform;
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
            if (_switchRequestSubscription != null)
            {
                _switchRequestSubscription.Dispose();
                _switchRequestSubscription = null;
            }

            if (!_isServiceRegistered)
            {
                return;
            }

            ServiceHub.Unregister<ITeamModule>(this);
            _isServiceRegistered = false;
        }
    }
}
