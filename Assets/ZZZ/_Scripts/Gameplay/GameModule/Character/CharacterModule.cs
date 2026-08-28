using System;
using System.Collections.Generic;

using SPFramework;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 统一驱动当前激活角色实例
    /// </summary>
    public sealed class CharacterModule : Module, ICharacterModule
    {
        private sealed class Registration
        {
            public readonly ICharacterUpdateTarget Target;
            public bool IsActive;

            public Registration(ICharacterUpdateTarget target)
            {
                Target = target;
                IsActive = true;
            }
        }

        private readonly Dictionary<ICharacterUpdateTarget, Registration> _registrations =
            new Dictionary<ICharacterUpdateTarget, Registration>();
        private readonly List<Registration> _registrationOrder = new List<Registration>();
        private readonly List<Registration> _executionSnapshot = new List<Registration>();

        private bool _isExecuting;
        private bool _isDestroyed;

        /// <inheritdoc/>
        public override void OnCreate()
        {
            _isDestroyed = false;
        }

        /// <inheritdoc/>
        public override void OnDestroy()
        {
            _isDestroyed = true;

            for (int index = 0; index < _registrationOrder.Count; index++)
            {
                _registrationOrder[index].IsActive = false;
            }

            _registrations.Clear();
            _registrationOrder.Clear();
            _executionSnapshot.Clear();
        }

        /// <inheritdoc/>
        public void Register(ICharacterUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (_isDestroyed)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterModule)} 已销毁 不能注册角色更新目标");
            }

            if (_registrations.ContainsKey(target))
            {
                throw new InvalidOperationException("角色更新目标不能重复注册");
            }

            Registration registration = new Registration(target);
            _registrations.Add(target, registration);
            _registrationOrder.Add(registration);
        }

        /// <inheritdoc/>
        public void Unregister(ICharacterUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (_isDestroyed)
            {
                return;
            }

            if (!_registrations.TryGetValue(target, out Registration registration))
            {
                throw new InvalidOperationException("角色更新目标尚未注册");
            }

            _registrations.Remove(target);
            registration.IsActive = false;

            if (!_isExecuting)
            {
                CompactRegistrations();
            }
        }

        /// <inheritdoc/>
        public void LogicUpdate(float tickDeltaSeconds)
        {
            if (_isDestroyed)
            {
                return;
            }

            BeginExecutionSnapshot();
            try
            {
                for (int index = 0; index < _executionSnapshot.Count; index++)
                {
                    Registration registration = _executionSnapshot[index];
                    if (registration.IsActive)
                    {
                        registration.Target.LogicUpdate(tickDeltaSeconds);
                    }
                }
            }
            finally
            {
                EndExecutionSnapshot();
            }
        }

        /// <inheritdoc/>
        public void RenderUpdate(float deltaTimeSeconds)
        {
            if (_isDestroyed)
            {
                return;
            }

            BeginExecutionSnapshot();
            try
            {
                for (int index = 0; index < _executionSnapshot.Count; index++)
                {
                    Registration registration = _executionSnapshot[index];
                    if (registration.IsActive)
                    {
                        registration.Target.RenderUpdate(deltaTimeSeconds);
                    }
                }
            }
            finally
            {
                EndExecutionSnapshot();
            }
        }

        private void BeginExecutionSnapshot()
        {
            _executionSnapshot.Clear();
            _executionSnapshot.AddRange(_registrationOrder);
            _isExecuting = true;
        }

        private void EndExecutionSnapshot()
        {
            _isExecuting = false;
            CompactRegistrations();
        }

        private void CompactRegistrations()
        {
            int activeCount = 0;
            for (int index = 0; index < _registrationOrder.Count; index++)
            {
                Registration registration = _registrationOrder[index];
                if (!registration.IsActive)
                {
                    continue;
                }

                _registrationOrder[activeCount] = registration;
                activeCount++;
            }

            int inactiveCount = _registrationOrder.Count - activeCount;
            if (inactiveCount > 0)
            {
                _registrationOrder.RemoveRange(activeCount, inactiveCount);
            }
        }
    }
}
