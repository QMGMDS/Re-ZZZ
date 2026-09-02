using System;
using System.Collections.Generic;

using GamePlay.Definition;

namespace GamePlay.Character
{
    public sealed class CharacterActionArbiter
    {
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> _linksBySourceActionId;

        public CharacterActionArbiter(CharacterActionSetAsset actionSet)
        {
            if (actionSet == null)
            {
                throw new ArgumentNullException(nameof(actionSet));
            }

            actionSet.BuildRuntimeLookups(
                out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
                out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId);
            _actionsById = actionsById;
            _linksBySourceActionId = linksBySourceActionId;
        }

        public CharacterActionArbiter(
            IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
            IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            _actionsById = actionsById ?? throw new ArgumentNullException(nameof(actionsById));
            _linksBySourceActionId =
                linksBySourceActionId ?? throw new ArgumentNullException(nameof(linksBySourceActionId));
        }

        public bool TrySelect(
            string currentActionId,
            float normalizedProgress,
            in CharacterIntention intention,
            in CharacterFact fact,
            out CharacterActionLink selectedLink,
            out CharacterActionAsset targetAction)
        {
            if (string.IsNullOrWhiteSpace(currentActionId))
            {
                throw new ArgumentException("当前动作 ID 不能为空", nameof(currentActionId));
            }

            if (float.IsNaN(normalizedProgress)
                || float.IsInfinity(normalizedProgress)
                || normalizedProgress < 0f
                || normalizedProgress > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(normalizedProgress));
            }

            intention.ValidateRuntime();
            fact.ValidateRuntime();

            if (!_actionsById.ContainsKey(currentActionId))
            {
                throw new InvalidOperationException(
                    $"当前动作不存在 {currentActionId}");
            }

            selectedLink = default;
            targetAction = null;

            if (!_linksBySourceActionId.TryGetValue(
                    currentActionId,
                    out IReadOnlyList<CharacterActionLink> outgoingLinks))
            {
                return false;
            }

            for (int index = 0; index < outgoingLinks.Count; index++)
            {
                CharacterActionLink link = outgoingLinks[index];
                if (normalizedProgress < link.NormalizedInterruptionWindowStart
                    || normalizedProgress > link.NormalizedInterruptionWindowEnd)
                {
                    continue;
                }

                if (!Matches(link.RequiredIntention.Move, intention.Move)
                    || !Matches(link.RequiredIntention.Attack, intention.Attack)
                    || !Matches(link.RequiredIntention.Evade, intention.Evade)
                    || !Matches(link.RequiredIntention.Skill, intention.Skill)
                    || !Matches(link.RequiredIntention.Ultimate, intention.Ultimate)
                    || !Matches(link.RequiredFact.EnterField, fact.EnterField)
                    || !Matches(link.RequiredFact.ExitField, fact.ExitField)
                    || !Matches(link.RequiredFact.Hit, fact.Hit)
                    || !Matches(link.RequiredFact.Death, fact.Death))
                {
                    continue;
                }

                if (!_actionsById.TryGetValue(link.TargetActionId, out targetAction))
                {
                    throw new InvalidOperationException(
                        $"动作链接目标不存在 {link.TargetActionId}");
                }

                selectedLink = link;
                return true;
            }

            return false;
        }

        private static bool Matches(Trilean required, Trilean actual)
        {
            return required == Trilean.DontCare || required == actual;
        }
    }
}
