using UnityEngine;

using NUnit.Framework;

using GamePlay.Data;
using GamePlay.GameModel;

namespace GamePlay.Tests
{
    public sealed class FixedStepAndInputCommandBufferTests
    {
        [Test]
        public void FixedStepClockAdvancesTwoTicksForOneSixtiethSecond()
        {
            FixedStepClock clock = new FixedStepClock(120, 8);
            int callbackCount = 0;
            float lastLogicalTimeSeconds = 0f;

            int tickCount = clock.Advance(
                1f / 60f,
                logicalTimeSeconds =>
                {
                    callbackCount++;
                    lastLogicalTimeSeconds = logicalTimeSeconds;
                });

            Assert.That(tickCount, Is.EqualTo(2));
            Assert.That(callbackCount, Is.EqualTo(2));
            Assert.That(lastLogicalTimeSeconds, Is.EqualTo(1f / 60f).Within(0.000001f));
        }

        [Test]
        public void FixedStepClockBoundsCatchUpAndDropsExcessTime()
        {
            FixedStepClock clock = new FixedStepClock(120, 3);

            int tickCount = clock.Advance(1f, _ => { });

            Assert.That(tickCount, Is.EqualTo(3));
            Assert.That(clock.DiscardedSeconds, Is.GreaterThan(0.9d));
            Assert.That(clock.Advance(0f, _ => { }), Is.EqualTo(0));
        }

        [Test]
        public void FixedStepClockKeepsFractionalRemainderAcrossCatchUp()
        {
            FixedStepClock clock = new FixedStepClock(10, 2);

            Assert.That(clock.Advance(0.05f, _ => { }), Is.EqualTo(0));
            Assert.That(clock.Advance(1f, _ => { }), Is.EqualTo(2));
            Assert.That(clock.DiscardedSeconds, Is.EqualTo(0.8d).Within(0.000001d));
            Assert.That(clock.Advance(0.05f, _ => { }), Is.EqualTo(1));
            Assert.That(clock.LogicalTimeSeconds, Is.EqualTo(0.3d).Within(0.000001d));
        }

        [Test]
        public void InputCommandBufferKeepsLatestMoveAndConsumesButtonsOnce()
        {
            InputCommandBuffer buffer = new InputCommandBuffer();
            buffer.Capture(new CharacterInputData(
                Vector2.up,
                true,
                true,
                true,
                true,
                true));
            buffer.Capture(new CharacterInputData(
                Vector2.right,
                false,
                false,
                false,
                false,
                false));

            CharacterInputData firstTickInput = buffer.Consume();
            CharacterInputData secondTickInput = buffer.Consume();

            Assert.That(firstTickInput.Move, Is.EqualTo(Vector2.right));
            Assert.That(firstTickInput.Attack, Is.True);
            Assert.That(firstTickInput.Evade, Is.True);
            Assert.That(firstTickInput.Skill, Is.True);
            Assert.That(firstTickInput.Ultimate, Is.True);
            Assert.That(firstTickInput.Switch, Is.True);
            Assert.That(secondTickInput.Move, Is.EqualTo(Vector2.right));
            Assert.That(secondTickInput.Attack, Is.False);
            Assert.That(secondTickInput.Evade, Is.False);
            Assert.That(secondTickInput.Skill, Is.False);
            Assert.That(secondTickInput.Ultimate, Is.False);
            Assert.That(secondTickInput.Switch, Is.False);
        }
    }
}
