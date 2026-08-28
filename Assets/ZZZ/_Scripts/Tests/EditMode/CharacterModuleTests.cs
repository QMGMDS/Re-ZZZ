using System;
using System.Reflection;

using NUnit.Framework;

using GamePlay.GameModule;

namespace GamePlay.Tests
{
    public sealed class CharacterModuleTests
    {
        private ICharacterModule _module;
        private object _moduleObject;
        private MethodInfo _createModule;
        private MethodInfo _destroyModule;

        [SetUp]
        public void SetUp()
        {
            Type moduleType = typeof(ICharacterModule).Assembly.GetType(
                "GamePlay.GameModule.CharacterModule");
            _moduleObject = Activator.CreateInstance(moduleType);
            _module = (ICharacterModule)_moduleObject;
            _createModule = moduleType.GetMethod("OnCreate");
            _destroyModule = moduleType.GetMethod("OnDestroy");
            _createModule.Invoke(_moduleObject, null);
        }

        [TearDown]
        public void TearDown()
        {
            _destroyModule.Invoke(_moduleObject, null);
        }

        [Test]
        public void RegisteringSameTargetTwiceThrows()
        {
            TestTarget target = new TestTarget();

            _module.Register(target);

            Assert.Throws<InvalidOperationException>(() => _module.Register(target));
        }

        [Test]
        public void RegisteredTargetReceivesLogicAndRenderUpdates()
        {
            TestTarget target = new TestTarget();

            _module.Register(target);
            _module.LogicUpdate(1f / 120f);
            _module.RenderUpdate(1f / 60f);

            Assert.That(target.LogicUpdateCount, Is.EqualTo(1));
            Assert.That(target.RenderUpdateCount, Is.EqualTo(1));
            Assert.That(target.LastLogicDeltaSeconds, Is.EqualTo(1f / 120f));
            Assert.That(target.LastRenderDeltaSeconds, Is.EqualTo(1f / 60f));
        }

        [Test]
        public void UnregisteringDuringLogicSkipsTargetInCurrentAndFollowingPhases()
        {
            TestTarget firstTarget = new TestTarget();
            TestTarget secondTarget = new TestTarget();
            firstTarget.LogicUpdated = () => _module.Unregister(secondTarget);

            _module.Register(firstTarget);
            _module.Register(secondTarget);

            _module.LogicUpdate(1f / 120f);
            _module.RenderUpdate(1f / 60f);

            Assert.That(firstTarget.LogicUpdateCount, Is.EqualTo(1));
            Assert.That(secondTarget.LogicUpdateCount, Is.EqualTo(0));
            Assert.That(secondTarget.RenderUpdateCount, Is.EqualTo(0));
        }

        [Test]
        public void RegisteringDuringUpdateWaitsForTheNextSnapshot()
        {
            TestTarget firstTarget = new TestTarget();
            TestTarget registeredTarget = new TestTarget();
            bool isRegistered = false;
            firstTarget.LogicUpdated = () =>
            {
                if (!isRegistered)
                {
                    isRegistered = true;
                    _module.Register(registeredTarget);
                }
            };

            _module.Register(firstTarget);
            _module.LogicUpdate(1f / 120f);

            Assert.That(registeredTarget.LogicUpdateCount, Is.EqualTo(0));

            _module.LogicUpdate(1f / 120f);

            Assert.That(registeredTarget.LogicUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisteringDuringRenderWaitsForTheNextRenderSnapshot()
        {
            TestTarget firstTarget = new TestTarget();
            TestTarget registeredTarget = new TestTarget();
            bool isRegistered = false;
            firstTarget.RenderUpdated = () =>
            {
                if (!isRegistered)
                {
                    isRegistered = true;
                    _module.Register(registeredTarget);
                }
            };

            _module.Register(firstTarget);
            _module.RenderUpdate(1f / 60f);

            Assert.That(registeredTarget.RenderUpdateCount, Is.EqualTo(0));

            _module.RenderUpdate(1f / 60f);

            Assert.That(registeredTarget.RenderUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void UnregisteringAfterModuleDestroyIsIgnored()
        {
            TestTarget target = new TestTarget();
            _module.Register(target);
            _destroyModule.Invoke(_moduleObject, null);

            Assert.DoesNotThrow(() => _module.Unregister(target));
        }

        private sealed class TestTarget : ICharacterUpdateTarget
        {
            public int LogicUpdateCount { get; private set; }
            public int RenderUpdateCount { get; private set; }
            public float LastLogicDeltaSeconds { get; private set; }
            public float LastRenderDeltaSeconds { get; private set; }
            public Action LogicUpdated { get; set; }
            public Action RenderUpdated { get; set; }

            public void LogicUpdate(float tickDeltaSeconds)
            {
                LogicUpdateCount++;
                LastLogicDeltaSeconds = tickDeltaSeconds;
                LogicUpdated?.Invoke();
            }

            public void RenderUpdate(float deltaTimeSeconds)
            {
                RenderUpdateCount++;
                LastRenderDeltaSeconds = deltaTimeSeconds;
                RenderUpdated?.Invoke();
            }
        }
    }
}
