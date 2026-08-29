using System;
using System.Collections.Generic;

using UnityEngine;

using GamePlay.Collider.Contract;

namespace GamePlay.Collider
{
    /// <summary>
    /// 统一驱动当前激活碰撞体实例
    /// </summary>
    public sealed class ColliderModule : IColliderModule
    {
        private readonly List<IColliderUpdateTarget> _targets =
            new List<IColliderUpdateTarget>();

        /// <inheritdoc/>
        public void Register(IColliderUpdateTarget target)
        {
            if (_targets.Contains(target))
            {
                throw new InvalidOperationException("碰撞体更新目标不能重复注册");
            }

            _targets.Add(target);
        }

        /// <inheritdoc/>
        public void Unregister(IColliderUpdateTarget target)
        {
            _targets.Remove(target);
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            Physics.SyncTransforms();

            for (int index = 0; index < _targets.Count; index++)
            {
                _targets[index].LogicUpdate(tickDeltaSeconds);
            }
        }
    }
}
