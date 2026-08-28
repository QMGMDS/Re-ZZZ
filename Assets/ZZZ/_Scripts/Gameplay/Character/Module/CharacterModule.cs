using System;
using System.Collections.Generic;

using GamePlay.Character.Contract;
using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 统一驱动当前激活角色实例
    /// </summary>
    public sealed class CharacterModule : ICharacterModule
    {
        private readonly List<ICharacterUpdateTarget> _targets =
            new List<ICharacterUpdateTarget>();
        private readonly Dictionary<ICharacterUpdateTarget, int> _entityIdsByTarget =
            new Dictionary<ICharacterUpdateTarget, int>();
        private readonly Dictionary<int, CharacterInfoRuntime> _characterInfoRuntimes =
            new Dictionary<int, CharacterInfoRuntime>();

        private int _nextEntityId;

        /// <inheritdoc/>
        public int Register(
            ICharacterUpdateTarget target,
            CharacterInfoRuntime characterInfoRuntime)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (characterInfoRuntime == null)
            {
                throw new ArgumentNullException(nameof(characterInfoRuntime));
            }

            if (_entityIdsByTarget.ContainsKey(target))
            {
                throw new InvalidOperationException("角色更新目标不能重复注册");
            }

            int entityId = _nextEntityId++;
            _targets.Add(target);
            _entityIdsByTarget.Add(target, entityId);
            _characterInfoRuntimes.Add(entityId, characterInfoRuntime);
            return entityId;
        }

        /// <inheritdoc/>
        public void Unregister(ICharacterUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!_entityIdsByTarget.TryGetValue(target, out int entityId))
            {
                // 无注册或者已被取消注册
                return;
            }

            _targets.Remove(target);
            _entityIdsByTarget.Remove(target);
            _characterInfoRuntimes.Remove(entityId);
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<int, CharacterInfoRuntime> GetCharacterInfoRuntimes()
        {
            return _characterInfoRuntimes;
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            for (int index = 0; index < _targets.Count; index++)
            {
                _targets[index].LogicUpdate(tickDeltaSeconds);
            }
        }

        /// <inheritdoc/>
        public void RenderUpdate(float deltaTimeSeconds)
        {
            for (int index = 0; index < _targets.Count; index++)
            {
                _targets[index].RenderUpdate(deltaTimeSeconds);
            }
        }
    }
}
