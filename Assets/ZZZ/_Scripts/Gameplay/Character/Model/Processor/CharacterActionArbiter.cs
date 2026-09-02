using System;
using System.Collections.Generic;

using GamePlay.Definition;

namespace GamePlay.Character
{
    public sealed class CharacterActionArbiter
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> _linksBySourceActionId;
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;

        public CharacterActionArbiter(
            IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId,
            IReadOnlyDictionary<string, CharacterActionAsset> actionsById)
        {
            _linksBySourceActionId = linksBySourceActionId;
            _actionsById = actionsById;
        }

        public void Arbitrate(ref CharacterActionState state)
        {
            CharacterActionAsset currentAction = _actionsById[state.CurrentActionId];

            // 进度归一化
            float normalizedProgress = state.LogicalProgressSeconds / currentAction.DurationSeconds;
            normalizedProgress = Math.Clamp(normalizedProgress, 0, 1);

            // 无去边
            if (!_linksBySourceActionId.TryGetValue(state.CurrentActionId, out IReadOnlyList<CharacterActionLink> outgoingLinks))
            {
                return;
            }

            for (int index = 0; index < outgoingLinks.Count; index++)
            {
                // 按优先级遍历
                CharacterActionLink link = outgoingLinks[index];

                // 打断窗口判断
                if (normalizedProgress < link.NormalizedInterruptionWindowStart
                    || normalizedProgress > link.NormalizedInterruptionWindowEnd)
                {
                    continue;
                }

                // 意图判断
                if (!Matches(link.RequiredIntention.Move, state.Intention.Move)
                    || !Matches(link.RequiredIntention.Attack, state.Intention.Attack)
                    || !Matches(link.RequiredIntention.Evade, state.Intention.Evade)
                    || !Matches(link.RequiredIntention.Skill, state.Intention.Skill)
                    || !Matches(link.RequiredIntention.Ultimate, state.Intention.Ultimate))
                {
                    continue;
                }

                // 事实判断
                if (!Matches(link.RequiredFact.SwitchIn, state.Fact.SwitchIn)
                    || !Matches(link.RequiredFact.SwitchOut, state.Fact.SwitchOut)
                    || !Matches(link.RequiredFact.Hit, state.Fact.Hit)
                    || !Matches(link.RequiredFact.Death, state.Fact.Death))
                {
                    continue;
                }

                // 动作切换
                state.SetCurrentActionId(link.TargetActionId);
                state.SetLogicalProgressSeconds(0f);
                state.SetFact(state.Fact.ConsumeRequired(link.RequiredFact));
                return;
            }
        }

        private static bool Matches(Trilean required, Trilean actual)
        {
            return required == Trilean.DontCare || required == actual;
        }
    }
}
