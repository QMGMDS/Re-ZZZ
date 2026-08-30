using System;
using System.Collections.Generic;

using GamePlay.Character;
using GamePlay.Character.Contract;
using GamePlay.Team.Contract;
using SPFramework;

namespace GamePlay.Team
{
    /// <summary>
    /// 统一管理队伍成员并路由玩家输入
    /// </summary>
    public sealed class TeamModule : ITeamModule
    {
        private readonly List<ITeamCharacter> _characters =
            new List<ITeamCharacter>();

        private ITeamCharacter _activeCharacter;
        private bool _fieldStatesInitialized;

        /// <inheritdoc/>
        public void Register(ITeamCharacter character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_characters.Contains(character))
            {
                throw new InvalidOperationException("队伍角色不能重复注册");
            }

            _characters.Add(character);

            if (_activeCharacter == null)
            {
                _activeCharacter = character;
            }

            if (_fieldStatesInitialized
                && !ReferenceEquals(_activeCharacter, character))
            {
                character.InitializeInactive();
            }
        }

        /// <inheritdoc/>
        public void Unregister(ITeamCharacter character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (!_characters.Remove(character))
            {
                return;
            }

            if (!ReferenceEquals(_activeCharacter, character))
            {
                return;
            }

            _activeCharacter = _characters.Count == 0
                ? null
                : _characters[0];
        }

        /// <summary>
        /// 驱动一次逻辑 Tick 的玩家输入路由
        /// </summary>
        /// <param name="logicalTimeSeconds">当前逻辑时间 单位为秒</param>
        public void LogicUpdate(float logicalTimeSeconds)
        {
            InitializeInactiveCharacters();

            if (!ServiceHub.TryGet<IPlayerInputRouter>(
                    out IPlayerInputRouter playerInputRouter))
            {
                return;
            }

            InputCharacterData inputCharacterData =
                playerInputRouter.ConsumePlayerInput(logicalTimeSeconds);

            if (_activeCharacter == null)
            {
                return;
            }

            if (inputCharacterData.Switch)
            {
                SwitchActiveCharacter();
            }

            _activeCharacter.ReceivePlayerInput(inputCharacterData);
        }

        private void InitializeInactiveCharacters()
        {
            if (_fieldStatesInitialized || _characters.Count == 0)
            {
                return;
            }

            for (int index = 0; index < _characters.Count; index++)
            {
                ITeamCharacter character = _characters[index];
                if (!ReferenceEquals(_activeCharacter, character))
                {
                    character.InitializeInactive();
                }
            }

            _fieldStatesInitialized = true;
        }

        private void SwitchActiveCharacter()
        {
            if (_characters.Count < 2)
            {
                return;
            }

            int activeCharacterIndex = _characters.IndexOf(_activeCharacter);
            int targetCharacterIndex = (activeCharacterIndex + 1) % _characters.Count;
            ITeamCharacter targetCharacter = _characters[targetCharacterIndex];

            _activeCharacter.ExitField();
            _activeCharacter = targetCharacter;
            _activeCharacter.EnterField();
        }
    }
}
