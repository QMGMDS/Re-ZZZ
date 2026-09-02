using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using GamePlay.Character.Public;

namespace GamePlay.Character
{
    public sealed class PlayerCharacterServiceRouter : ICharacterService, IDisposable
    {
        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            public static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();

            public bool Equals(T left, T right)
            {
                return ReferenceEquals(left, right);
            }

            public int GetHashCode(T value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }

        private readonly Dictionary<int, IPlayerCharacterService> _unitsByEntityId =
            new Dictionary<int, IPlayerCharacterService>();
        private readonly Dictionary<IPlayerCharacterService, int> _entityIdsByUnit =
            new Dictionary<IPlayerCharacterService, int>(ReferenceComparer<IPlayerCharacterService>.Instance);
        private readonly HashSet<IPlayerCharacterService> _exitingUnits =
            new HashSet<IPlayerCharacterService>(ReferenceComparer<IPlayerCharacterService>.Instance);
        private readonly HashSet<IPlayerCharacterService> _pendingTickRemovals =
            new HashSet<IPlayerCharacterService>(ReferenceComparer<IPlayerCharacterService>.Instance);
        private readonly List<IPlayerCharacterService> _tickSnapshot =
            new List<IPlayerCharacterService>();

        private IPlayerCharacterService _currentUnit;
        private int _nextEntityId;
        private bool _isUpdating;
        private bool _isDisposed;

        public IPlayerCharacterService CurrentCharacter => _currentUnit;

        public int RegisteredUnitCount => _unitsByEntityId.Count;

        public int RegisterRuntimeUnit(IPlayerCharacterService runtimeUnit)
        {
            EnsureNotDisposed();
            EnsureRuntimeUnit(runtimeUnit);

            if (_entityIdsByUnit.TryGetValue(runtimeUnit, out int existingEntityId))
            {
                return existingEntityId;
            }

            if (_nextEntityId == int.MaxValue)
            {
                throw new InvalidOperationException("角色实体 ID 已耗尽");
            }

            int entityId = _nextEntityId++;
            _entityIdsByUnit.Add(runtimeUnit, entityId);
            _unitsByEntityId.Add(entityId, runtimeUnit);
            return entityId;
        }

        public bool TryGetEntityId(IPlayerCharacterService runtimeUnit, out int entityId)
        {
            EnsureNotDisposed();
            EnsureRuntimeUnit(runtimeUnit);
            return _entityIdsByUnit.TryGetValue(runtimeUnit, out entityId);
        }

        public bool TryGetRuntimeUnit(int entityId, out IPlayerCharacterService runtimeUnit)
        {
            EnsureNotDisposed();
            return _unitsByEntityId.TryGetValue(entityId, out runtimeUnit);
        }

        public void SuspendRuntimeUnit(IPlayerCharacterService runtimeUnit)
        {
            EnsureNotDisposed();
            EnsureRegistered(runtimeUnit);

            if (_isUpdating)
            {
                _pendingTickRemovals.Add(runtimeUnit);
                return;
            }

            RemoveFromTickCollections(runtimeUnit);
        }

        public void UnregisterRuntimeUnit(IPlayerCharacterService runtimeUnit)
        {
            EnsureNotDisposed();
            EnsureRegistered(runtimeUnit);

            int entityId = _entityIdsByUnit[runtimeUnit];
            _entityIdsByUnit.Remove(runtimeUnit);
            _unitsByEntityId.Remove(entityId);

            if (_isUpdating)
            {
                _pendingTickRemovals.Add(runtimeUnit);
                return;
            }

            RemoveFromTickCollections(runtimeUnit);
        }

        public void EnterField(int characterEntityId, Transform characterTransform)
        {
            EnsureNotDisposed();

            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            if (_currentUnit != null)
            {
                throw new InvalidOperationException("已有角色在场 不能重复上场");
            }

            if (!_unitsByEntityId.TryGetValue(characterEntityId, out IPlayerCharacterService targetUnit))
            {
                throw new InvalidOperationException(
                    $"角色实体 ID 未注册 {characterEntityId}");
            }

            if (_exitingUnits.Contains(targetUnit)
                || _pendingTickRemovals.Contains(targetUnit))
            {
                throw new InvalidOperationException(
                    $"角色实体 ID 正在退场 不能上场 {characterEntityId}");
            }

            targetUnit.EnterField(characterTransform);
            _currentUnit = targetUnit;
        }

        public void ExitField()
        {
            EnsureNotDisposed();

            if (_currentUnit == null)
            {
                throw new InvalidOperationException("当前没有在场角色 不能退场");
            }

            IPlayerCharacterService exitingUnit = _currentUnit;
            _currentUnit = null;
            _exitingUnits.Add(exitingUnit);

            try
            {
                exitingUnit.ExitField();
            }
            catch
            {
                _exitingUnits.Remove(exitingUnit);
                _currentUnit = exitingUnit;
                throw;
            }
        }

        public void CharacterUpdate()
        {
            EnsureNotDisposed();

            _tickSnapshot.Clear();
            if (_currentUnit != null)
            {
                _tickSnapshot.Add(_currentUnit);
            }

            foreach (IPlayerCharacterService exitingUnit in _exitingUnits)
            {
                if (!ReferenceEquals(exitingUnit, _currentUnit))
                {
                    _tickSnapshot.Add(exitingUnit);
                }
            }

            _isUpdating = true;
            try
            {
                for (int index = 0; index < _tickSnapshot.Count; index++)
                {
                    IPlayerCharacterService runtimeUnit = _tickSnapshot[index];
                    if (!_pendingTickRemovals.Contains(runtimeUnit))
                    {
                        runtimeUnit.CharacterUpdate();
                    }
                }
            }
            finally
            {
                _isUpdating = false;
                ApplyPendingTickRemovals();
                _tickSnapshot.Clear();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _currentUnit = null;
            _exitingUnits.Clear();
            _pendingTickRemovals.Clear();
            _tickSnapshot.Clear();
            _entityIdsByUnit.Clear();
            _unitsByEntityId.Clear();
            _isDisposed = true;
        }

        private void ApplyPendingTickRemovals()
        {
            foreach (IPlayerCharacterService runtimeUnit in _pendingTickRemovals)
            {
                RemoveFromTickCollections(runtimeUnit);
            }

            _pendingTickRemovals.Clear();
        }

        private void RemoveFromTickCollections(IPlayerCharacterService runtimeUnit)
        {
            if (ReferenceEquals(_currentUnit, runtimeUnit))
            {
                _currentUnit = null;
            }

            _exitingUnits.Remove(runtimeUnit);
        }

        private void EnsureRegistered(IPlayerCharacterService runtimeUnit)
        {
            EnsureRuntimeUnit(runtimeUnit);
            if (!_entityIdsByUnit.ContainsKey(runtimeUnit))
            {
                throw new InvalidOperationException("角色运行时单元尚未注册");
            }
        }

        private static void EnsureRuntimeUnit(IPlayerCharacterService runtimeUnit)
        {
            if (runtimeUnit == null)
            {
                throw new ArgumentNullException(nameof(runtimeUnit));
            }
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(PlayerCharacterServiceRouter));
            }
        }
    }
}
