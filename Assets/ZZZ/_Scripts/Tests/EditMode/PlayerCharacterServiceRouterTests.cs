using System;
using System.Collections.Generic;

using UnityEngine;

using NUnit.Framework;

using GamePlay.Character;
using GamePlay.Character.Public;

namespace GamePlay.Tests
{
    public sealed class PlayerCharacterServiceRouterTests
    {
        private readonly List<GameObject> _createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _createdObjects.Count - 1; index >= 0; index--)
            {
                if (_createdObjects[index] != null)
                {
                    UnityEngine.Object.DestroyImmediate(_createdObjects[index]);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void RegisterKeepsEntityIdStableAndDoesNotReuseIds()
        {
            PlayerCharacterServiceRouter router = new PlayerCharacterServiceRouter();
            FakePlayerCharacterService first = new FakePlayerCharacterService();
            FakePlayerCharacterService second = new FakePlayerCharacterService();
            FakePlayerCharacterService third = new FakePlayerCharacterService();

            int firstId = router.RegisterRuntimeUnit(first);
            int secondId = router.RegisterRuntimeUnit(second);
            router.SuspendRuntimeUnit(first);

            Assert.That(router.RegisterRuntimeUnit(first), Is.EqualTo(firstId));

            router.UnregisterRuntimeUnit(first);
            int thirdId = router.RegisterRuntimeUnit(third);

            Assert.That(secondId, Is.GreaterThan(firstId));
            Assert.That(thirdId, Is.GreaterThan(secondId));
        }

        [Test]
        public void UpdateTicksCurrentAndExitingUnitsTogether()
        {
            PlayerCharacterServiceRouter router = new PlayerCharacterServiceRouter();
            FakePlayerCharacterService exiting = new FakePlayerCharacterService();
            FakePlayerCharacterService current = new FakePlayerCharacterService();
            int exitingId = router.RegisterRuntimeUnit(exiting);
            int currentId = router.RegisterRuntimeUnit(current);
            Transform source = CreateGameObject("Source").transform;

            router.EnterField(exitingId, source);
            router.ExitField();
            router.EnterField(currentId, source);
            router.CharacterUpdate();

            Assert.That(exiting.UpdateCount, Is.EqualTo(1));
            Assert.That(current.UpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void InvalidEnterAndExitCallsThrow契约异常()
        {
            PlayerCharacterServiceRouter router = new PlayerCharacterServiceRouter();
            FakePlayerCharacterService first = new FakePlayerCharacterService();
            FakePlayerCharacterService second = new FakePlayerCharacterService();
            int firstId = router.RegisterRuntimeUnit(first);
            int secondId = router.RegisterRuntimeUnit(second);
            Transform source = CreateGameObject("Source").transform;

            Assert.Throws<ArgumentNullException>(() => router.EnterField(firstId, null));
            Assert.Throws<InvalidOperationException>(() => router.EnterField(999, source));
            Assert.Throws<InvalidOperationException>(() => router.ExitField());

            router.EnterField(firstId, source);
            Assert.Throws<InvalidOperationException>(() => router.EnterField(secondId, source));
            router.ExitField();
            Assert.Throws<InvalidOperationException>(() => router.EnterField(firstId, source));

            router.SuspendRuntimeUnit(first);
            router.EnterField(firstId, source);
        }

        [Test]
        public void SuspendedExitingUnitCanEnterAgainWithSameEntityId()
        {
            PlayerCharacterServiceRouter router = new PlayerCharacterServiceRouter();
            FakePlayerCharacterService unit = new FakePlayerCharacterService();
            int entityId = router.RegisterRuntimeUnit(unit);
            Transform source = CreateGameObject("Source").transform;

            router.EnterField(entityId, source);
            router.ExitField();
            router.SuspendRuntimeUnit(unit);
            router.EnterField(entityId, source);

            Assert.That(router.CurrentCharacter, Is.SameAs(unit));
            Assert.That(router.RegisterRuntimeUnit(unit), Is.EqualTo(entityId));
        }

        [Test]
        public void TickRemovalIsDeferredUntilIterationCompletes()
        {
            PlayerCharacterServiceRouter router = new PlayerCharacterServiceRouter();
            FakePlayerCharacterService unit = new FakePlayerCharacterService();
            int entityId = router.RegisterRuntimeUnit(unit);
            Transform source = CreateGameObject("Source").transform;
            unit.OnUpdate = updatedUnit => router.SuspendRuntimeUnit(updatedUnit);

            router.EnterField(entityId, source);
            router.CharacterUpdate();
            router.CharacterUpdate();

            Assert.That(unit.UpdateCount, Is.EqualTo(1));
            Assert.That(router.CurrentCharacter, Is.Null);
            Assert.That(router.RegisteredUnitCount, Is.EqualTo(1));
        }

        private GameObject CreateGameObject(string objectName)
        {
            GameObject gameObject = new GameObject(objectName);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class FakePlayerCharacterService : IPlayerCharacterService
        {
            public int UpdateCount { get; private set; }

            public Action<FakePlayerCharacterService> OnUpdate { get; set; }

            public void CharacterUpdate()
            {
                UpdateCount++;
                OnUpdate?.Invoke(this);
            }

            public void EnterField(Transform characterTransform)
            {
            }

            public void ExitField()
            {
            }
        }
    }
}
