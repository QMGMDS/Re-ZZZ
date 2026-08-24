using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using GamePlay.Data;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 根据角色意图和事实裁决本帧目标动作
    /// </summary>
    public sealed class CharacterActionArbiter
    {
        private readonly ReadOnlyCollection<CharacterActionAsset> _actions;

        public CharacterActionArbiter(IReadOnlyList<CharacterActionAsset> actions)
        {
            _actions = Array.AsReadOnly(actions.OrderByDescending(action => action.Priority).ToArray());
        }

        /// <summary>
        /// 返回优先级最高且满足全部条件的动作资产
        /// 无匹配动作时返回 null
        /// </summary>
        public CharacterActionAsset TrySwitch(in CharacterIntention intention, in CharacterFact fact)
        {
            for (int index = 0; index < _actions.Count; index++)
            {
                CharacterActionAsset action = _actions[index];
                CharacterIntention requiredIntention = action.RequiredIntention;
                CharacterFact requiredFact = action.RequiredFact;

                if (Matches(requiredIntention.Attack, intention.Attack)
                    && Matches(requiredIntention.Move, intention.Move)
                    && Matches(requiredFact.Death, fact.Death)
                    && Matches(requiredFact.LogicalProgress, fact.LogicalProgress))
                {
                    return action;
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
