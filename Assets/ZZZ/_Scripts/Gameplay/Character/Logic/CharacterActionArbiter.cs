using System.Collections.Generic;

namespace GamePlay.Character
{
    /// <summary>
    /// 根据角色意图和事实裁决本帧目标动作
    /// </summary>
    public sealed class CharacterActionArbiter
    {
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> _linksBySourceActionId;

        public CharacterActionArbiter(
            IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
            IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            _actionsById = actionsById;
            _linksBySourceActionId = linksBySourceActionId;
        }

        /// <summary>
        /// 返回当前动作出边中优先级最高且满足全部条件的目标动作资产
        /// 无匹配动作时返回 null
        /// </summary>
        public CharacterActionAsset TrySwitch(
            string currentActionId,
            float currentActionProgress,
            in CharacterIntention intention,
            in CharacterFact fact)
        {
            if (!_linksBySourceActionId.TryGetValue(currentActionId, out IReadOnlyList<CharacterActionLink> outgoingLinks))
            {
                return null;
            }

            // 按优先级遍历
            for (int index = 0; index < outgoingLinks.Count; index++)
            {
                CharacterActionLink link = outgoingLinks[index];
                // 打断窗口排除
                if (currentActionProgress < link.InterruptWindowStartProgress
                    || currentActionProgress > link.InterruptWindowEndProgress)
                {
                    continue;
                }

                // 条件匹配
                if (Matches(link.RequiredIntention.Move, intention.Move)
                    && Matches(link.RequiredIntention.Attack, intention.Attack)
                    && Matches(link.RequiredIntention.Evade, intention.Evade)
                    && Matches(link.RequiredIntention.Skill, intention.Skill)
                    && Matches(link.RequiredIntention.Ultimate, intention.Ultimate)
                    && Matches(link.RequiredIntention.Switch, intention.Switch)
                    && Matches(link.RequiredFact.Death, fact.Death))
                {
                    return _actionsById[link.ToActionId];
                }
            }

            return null;
        }

        private static bool Matches(Trilean required, Trilean actual)
        {
            return required == Trilean.DontCare || required == actual;
        }
    }
}
