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

        /// <inheritdoc/>
        public void Register(ICharacterUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (_targets.Contains(target))
            {
                throw new InvalidOperationException("角色更新目标不能重复注册");
            }

            _targets.Add(target);
        }

        /// <inheritdoc/>
        public void Unregister(ICharacterUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!_targets.Remove(target))
            {
                // 无注册或者已被取消注册
                return;
            }
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
