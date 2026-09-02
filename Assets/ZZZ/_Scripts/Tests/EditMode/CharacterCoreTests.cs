using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using NUnit.Framework;

using GamePlay.Character;
using GamePlay.Definition;

namespace GamePlay.Tests
{
    public sealed class CharacterCoreTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>();

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
        public void ArbiterMatchesDontCareCondition()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionAsset attack = CreateAction("attack");
            CharacterActionLink link = CreateLink(
                "idle",
                "attack",
                0f,
                1f,
                0,
                new CharacterIntention(
                    Trilean.DontCare,
                    Trilean.True,
                    Trilean.DontCare,
                    Trilean.DontCare,
                    Trilean.DontCare),
                CharacterFactConditionDontCare());
            CharacterActionSetAsset set = CreateSet(idle, new[] { idle, attack }, new[] { link });
            CharacterActionArbiter arbiter = new CharacterActionArbiter(set);

            bool selected = arbiter.TrySelect(
                "idle",
                0.5f,
                new CharacterIntention(
                    Trilean.False,
                    Trilean.True,
                    Trilean.False,
                    Trilean.False,
                    Trilean.False),
                CharacterFact.AllFalse,
                out CharacterActionLink selectedLink,
                out CharacterActionAsset targetAction);

            Assert.That(selected, Is.True);
            Assert.That(selectedLink.TargetActionId, Is.EqualTo("attack"));
            Assert.That(targetAction, Is.SameAs(attack));
        }

        [Test]
        public void ArbiterMatchesIntentionAndFactTogether()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionAsset hit = CreateAction("hit");
            CharacterActionLink link = CreateLink(
                "idle",
                "hit",
                0f,
                1f,
                0,
                new CharacterIntention(
                    Trilean.False,
                    Trilean.True,
                    Trilean.False,
                    Trilean.False,
                    Trilean.False),
                new CharacterFact(
                    Trilean.DontCare,
                    Trilean.DontCare,
                    Trilean.True,
                    Trilean.DontCare));
            CharacterActionSetAsset set = CreateSet(idle, new[] { idle, hit }, new[] { link });
            CharacterActionArbiter arbiter = new CharacterActionArbiter(set);
            CharacterFact fact = CharacterFact.AllFalse.MarkHit();

            bool selected = arbiter.TrySelect(
                "idle",
                0f,
                new CharacterIntention(
                    Trilean.False,
                    Trilean.True,
                    Trilean.False,
                    Trilean.False,
                    Trilean.False),
                fact,
                out _,
                out CharacterActionAsset targetAction);

            Assert.That(selected, Is.True);
            Assert.That(targetAction, Is.SameAs(hit));
        }

        [Test]
        public void ArbiterUsesPriorityAndStableListOrder()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionAsset first = CreateAction("first");
            CharacterActionAsset second = CreateAction("second");
            CharacterActionLink low = CreateLink("idle", "second", 0f, 1f, 1);
            CharacterActionLink highFirst = CreateLink("idle", "first", 0f, 1f, 2);
            CharacterActionLink highSecond = CreateLink("idle", "second", 0f, 1f, 2);
            CharacterActionSetAsset set = CreateSet(
                idle,
                new[] { idle, first, second },
                new[] { low, highFirst, highSecond });
            CharacterActionArbiter arbiter = new CharacterActionArbiter(set);

            bool selected = arbiter.TrySelect(
                "idle",
                0.5f,
                CharacterIntention.AllFalse,
                CharacterFact.AllFalse,
                out _,
                out CharacterActionAsset targetAction);

            Assert.That(selected, Is.True);
            Assert.That(targetAction, Is.SameAs(first));
        }

        [Test]
        public void ArbiterUsesActionListOrderForEqualPriorityTargets()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionAsset first = CreateAction("first");
            CharacterActionAsset second = CreateAction("second");
            CharacterActionLink secondLink = CreateLink("idle", "second", 0f, 1f, 3);
            CharacterActionLink firstLink = CreateLink("idle", "first", 0f, 1f, 3);
            CharacterActionSetAsset set = CreateSet(
                idle,
                new[] { idle, first, second },
                new[] { secondLink, firstLink });
            CharacterActionArbiter arbiter = new CharacterActionArbiter(set);

            bool selected = arbiter.TrySelect(
                "idle",
                0.5f,
                CharacterIntention.AllFalse,
                CharacterFact.AllFalse,
                out _,
                out CharacterActionAsset targetAction);

            Assert.That(selected, Is.True);
            Assert.That(targetAction, Is.SameAs(first));
        }

        [Test]
        public void ArbiterIncludesBothWindowBoundaries()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionAsset target = CreateAction("target");
            CharacterActionLink link = CreateLink("idle", "target", 0.25f, 0.75f, 0);
            CharacterActionSetAsset set = CreateSet(idle, new[] { idle, target }, new[] { link });
            CharacterActionArbiter arbiter = new CharacterActionArbiter(set);

            Assert.That(TrySelectTarget(arbiter, 0.25f), Is.SameAs(target));
            Assert.That(TrySelectTarget(arbiter, 0.75f), Is.SameAs(target));
            Assert.That(TrySelectTarget(arbiter, 0.2f), Is.Null);
            Assert.That(TrySelectTarget(arbiter, 0.8f), Is.Null);
        }

        [Test]
        public void TransitionConsumesOnlyFactsRequiredBySelectedLink()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionAsset target = CreateAction("target");
            CharacterActionLink link = CreateLink(
                "idle",
                "target",
                0f,
                1f,
                0,
                CharacterIntentionConditionDontCare(),
                new CharacterFact(
                    Trilean.False,
                    Trilean.DontCare,
                    Trilean.True,
                    Trilean.DontCare));
            CharacterActionSetAsset set = CreateSet(idle, new[] { idle, target }, new[] { link });
            CharacterActionTransition transition = new CharacterActionTransition(set);
            CharacterActionState state = CharacterActionState.CreateInitial("idle");
            state.Fact = CharacterFact.AllFalse.MarkHit().MarkEnterField();
            transition.ApplySelectedLink(ref state, link);

            Assert.That(state.Fact.Hit, Is.EqualTo(Trilean.False));
            Assert.That(state.Fact.EnterField, Is.EqualTo(Trilean.True));
        }

        [Test]
        public void TransitionRestartsWhenTargetHasSameId()
        {
            CharacterActionAsset idle = CreateAction("idle");
            CharacterActionLink link = CreateLink("idle", "idle", 0f, 1f, 0);
            CharacterActionSetAsset set = CreateSet(idle, new[] { idle }, new[] { link });
            CharacterActionTransition transition = new CharacterActionTransition(set);
            CharacterActionState state = CharacterActionState.CreateInitial("idle");
            state.LogicalProgressSeconds = 0.8f;

            transition.ApplySelectedLink(ref state, link);

            Assert.That(state.CurrentActionId, Is.EqualTo("idle"));
            Assert.That(state.LogicalProgressSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void TransitionCapsProgressAtActionDuration()
        {
            CharacterActionAsset action = CreateAction("action", 1f);
            CharacterActionSetAsset set = CreateSet(action, new[] { action }, Array.Empty<CharacterActionLink>());
            CharacterActionTransition transition = new CharacterActionTransition(set);
            CharacterActionState state = CharacterActionState.CreateInitial("action");

            transition.Advance(ref state, 2f);

            Assert.That(state.LogicalProgressSeconds, Is.EqualTo(1f));
        }

        [Test]
        public void RotationDriverSupportsAllDirectionModes()
        {
            GameObject character = CreateGameObject("Character");
            CharacterRotationDriver driver = new CharacterRotationDriver(character.transform);

            CharacterActionAsset live = CreateAction("live", 1f, ActionDirectionMode.LiveMoveDirection);
            CharacterActionState liveState = CharacterActionState.CreateInitial("live");
            liveState.MoveDirectionInWorld = Vector2.right;
            driver.UpdateActionDirection(ref liveState, live, true);
            Assert.That(liveState.ActionDirectionInWorld, Is.EqualTo(Vector2.right));

            CharacterActionAsset captureMove =
                CreateAction("captureMove", 1f, ActionDirectionMode.CaptureMoveDirectionOnEnter);
            CharacterActionState captureMoveState = CharacterActionState.CreateInitial("captureMove");
            captureMoveState.MoveDirectionInWorld = Vector2.up;
            driver.UpdateActionDirection(ref captureMoveState, captureMove, true);
            Assert.That(captureMoveState.ActionDirectionInWorld, Is.EqualTo(Vector2.up));

            CharacterActionAsset captureFacing =
                CreateAction("captureFacing", 1f, ActionDirectionMode.CaptureFacingDirectionOnEnter);
            CharacterActionState captureFacingState = CharacterActionState.CreateInitial("captureFacing");
            driver.UpdateActionDirection(ref captureFacingState, captureFacing, true);
            Assert.That(captureFacingState.ActionDirectionInWorld, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void RotationDriverRejectsInvalidCaptureDirections()
        {
            GameObject character = CreateGameObject("Character");
            CharacterRotationDriver driver = new CharacterRotationDriver(character.transform);
            CharacterActionAsset captureMove =
                CreateAction("captureMove", 1f, ActionDirectionMode.CaptureMoveDirectionOnEnter);
            CharacterActionState moveState = CharacterActionState.CreateInitial("captureMove");
            moveState.MoveDirectionInWorld = Vector2.zero;

            Assert.Throws<InvalidOperationException>(
                () => driver.UpdateActionDirection(ref moveState, captureMove, true));

            character.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            CharacterActionAsset captureFacing =
                CreateAction("captureFacing", 1f, ActionDirectionMode.CaptureFacingDirectionOnEnter);
            CharacterActionState facingState = CharacterActionState.CreateInitial("captureFacing");

            Assert.Throws<InvalidOperationException>(
                () => driver.UpdateActionDirection(ref facingState, captureFacing, true));
        }

        [Test]
        public void PositionDriverCalculatesCumulativeCurveDelta()
        {
            CharacterActionAsset action = CreateAction("move", 2f);
            SetField(
                action,
                "_cumulativeForwardDisplacement",
                AnimationCurve.Linear(0f, 0f, 2f, 4f));

            Vector3 displacement = CharacterPositionDriver.CalculateDisplacement(
                action,
                0.5f,
                1.5f,
                Vector2.up);

            Assert.That(displacement, Is.EqualTo(new Vector3(0f, 0f, 2f)));
        }

        [Test]
        public void CharacterInfoCalculatorUsesBaseValuesAsCurrentValues()
        {
            CharacterInfoAsset asset = CreateInstance<CharacterInfoAsset>();
            SetField(asset, "_characterConfigId", "hero");
            SetField(asset, "_baseHealth", 100);
            SetField(asset, "_baseAttack", 25);
            GamePlay.Character.CharacterInfo info =
                new CharacterInfoCalculator().CalculateInitialInfo(asset, 7);

            Assert.That(info.CharacterConfigId, Is.EqualTo("hero"));
            Assert.That(info.EntityId, Is.EqualTo(7));
            Assert.That(info.BaseHealth, Is.EqualTo(100));
            Assert.That(info.CurrentHealth, Is.EqualTo(100));
            Assert.That(info.BaseAttack, Is.EqualTo(25));
            Assert.That(info.CurrentAttack, Is.EqualTo(25));
        }

        private CharacterActionAsset CreateAction(
            string actionId,
            float durationSeconds = 1f,
            ActionDirectionMode directionMode = ActionDirectionMode.LiveMoveDirection)
        {
            CharacterActionAsset action = CreateInstance<CharacterActionAsset>();
            SetField(action, "_actionId", actionId);
            SetField(action, "_durationSeconds", durationSeconds);
            SetField(action, "_animationClip", CreateAnimationClip());
            SetField(action, "_actionDirectionMode", directionMode);
            SetField(action, "_maxRotationSpeedDegreesPerSecond", 360f);
            SetField(
                action,
                "_cumulativeForwardDisplacement",
                AnimationCurve.Linear(0f, 0f, durationSeconds, durationSeconds));
            return action;
        }

        private CharacterActionSetAsset CreateSet(
            CharacterActionAsset initialAction,
            CharacterActionAsset[] actions,
            CharacterActionLink[] links)
        {
            CharacterActionSetAsset set = CreateInstance<CharacterActionSetAsset>();
            SetField(set, "_initialAction", initialAction);
            SetField(set, "_actions", new List<CharacterActionAsset>(actions));
            SetField(set, "_links", new List<CharacterActionLink>(links));
            return set;
        }

        private CharacterActionLink CreateLink(
            string sourceActionId,
            string targetActionId,
            float start,
            float end,
            int priority,
            CharacterIntention? requiredIntention = null,
            CharacterFact? requiredFact = null)
        {
            return new CharacterActionLink(
                sourceActionId,
                targetActionId,
                start,
                end,
                priority,
                requiredIntention ?? CharacterIntentionConditionDontCare(),
                requiredFact ?? CharacterFactConditionDontCare(),
                0f);
        }

        private static CharacterIntention CharacterIntentionConditionDontCare()
        {
            return new CharacterIntention(
                Trilean.DontCare,
                Trilean.DontCare,
                Trilean.DontCare,
                Trilean.DontCare,
                Trilean.DontCare);
        }

        private static CharacterFact CharacterFactConditionDontCare()
        {
            return new CharacterFact(
                Trilean.DontCare,
                Trilean.DontCare,
                Trilean.DontCare,
                Trilean.DontCare);
        }

        private static CharacterActionAsset TrySelectTarget(
            CharacterActionArbiter arbiter,
            float normalizedProgress)
        {
            bool selected = arbiter.TrySelect(
                "idle",
                normalizedProgress,
                CharacterIntention.AllFalse,
                CharacterFact.AllFalse,
                out _,
                out CharacterActionAsset targetAction);
            return selected ? targetAction : null;
        }

        private AnimationClip CreateAnimationClip()
        {
            AnimationClip clip = new AnimationClip();
            _createdObjects.Add(clip);
            return clip;
        }

        private GameObject CreateGameObject(string objectName)
        {
            GameObject gameObject = new GameObject(objectName);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        private T CreateInstance<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            _createdObjects.Add(instance);
            return instance;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException($"未找到测试字段 {fieldName}");
            }

            field.SetValue(target, value);
        }
    }
}
