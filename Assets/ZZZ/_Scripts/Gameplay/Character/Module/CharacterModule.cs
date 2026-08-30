using System;
using System.Collections.Generic;

using GamePlay.Character.Contract;

namespace GamePlay.Character
{
    /// <summary>
    /// 统一驱动当前激活角色实例
    /// </summary>
    public sealed class CharacterModule : ICharacterModule
    {
        private readonly List<ICharacterUpdateTarget> _targets =
            new List<ICharacterUpdateTarget>();
        private readonly List<ICharacterUpdateTarget> _logicUpdateTargets =
            new List<ICharacterUpdateTarget>();
        private readonly Dictionary<ICharacterUpdateTarget, int> _entityIdsByTarget =
            new Dictionary<ICharacterUpdateTarget, int>();
        private readonly Dictionary<int, CharacterInfoRuntime> _characterInfoRuntimes =
            new Dictionary<int, CharacterInfoRuntime>();
        private readonly Dictionary<int, ICharacterHurtReceiver> _hurtReceiversByEntityId =
            new Dictionary<int, ICharacterHurtReceiver>();

        private int _nextEntityId;

        /// <inheritdoc/>
        public int Register(
            ICharacterUpdateTarget target,
            ICharacterHurtReceiver hurtReceiver,
            CharacterInfoRuntime characterInfoRuntime)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (hurtReceiver == null || characterInfoRuntime == null)
            {
                throw new ArgumentNullException(
                    hurtReceiver == null
                        ? nameof(hurtReceiver)
                        : nameof(characterInfoRuntime));
            }

            if (_entityIdsByTarget.ContainsKey(target))
            {
                throw new InvalidOperationException("角色更新目标不能重复注册");
            }

            int entityId = _nextEntityId++;
            _targets.Add(target);
            _entityIdsByTarget.Add(target, entityId);
            _characterInfoRuntimes.Add(entityId, characterInfoRuntime);
            _hurtReceiversByEntityId.Add(entityId, hurtReceiver);
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
            _hurtReceiversByEntityId.Remove(entityId);
        }

        /// <inheritdoc/>
        public bool TryGetCharacterInfoRuntime(
            int entityId,
            out CharacterInfoRuntime characterInfoRuntime)
        {
            return _characterInfoRuntimes.TryGetValue(
                entityId,
                out characterInfoRuntime);
        }

        /// <inheritdoc/>
        public bool TryGetCharacterHurtReceiver(
            int entityId,
            out ICharacterHurtReceiver hurtReceiver)
        {
            return _hurtReceiversByEntityId.TryGetValue(entityId, out hurtReceiver);
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            _logicUpdateTargets.Clear();
            _logicUpdateTargets.AddRange(_targets);

            for (int index = 0; index < _logicUpdateTargets.Count; index++)
            {
                ICharacterUpdateTarget target = _logicUpdateTargets[index];
                if (_entityIdsByTarget.ContainsKey(target))
                {
                    target.LogicUpdate(tickDeltaSeconds);
                }
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
