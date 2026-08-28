using UnityEngine;

using NUnit.Framework;

using GamePlay.Input;

namespace Tests
{
    public sealed class InputCommandBufferTests
    {
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
