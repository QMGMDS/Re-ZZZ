using NUnit.Framework;

using GamePlay.Root;

namespace Tests
{
    public sealed class FixedStepClockTests
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
    }
}
