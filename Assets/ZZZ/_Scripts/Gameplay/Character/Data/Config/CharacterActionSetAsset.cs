using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

namespace GamePlay.Character
{
    [Serializable]
    public struct CharacterActionLink
    {
        [SerializeField, Tooltip("出边")]
        private string _sourceActionId;
        [SerializeField, Tooltip("去边")]
        private string _targetActionId;
        [SerializeField, Tooltip("优先级")]
        private int _priority;
        [SerializeField, Range(0f, 1f), Tooltip("打断窗口起点")]
        private float _normalizedInterruptionWindowStart;
        [SerializeField, Range(0f, 1f), Tooltip("打断窗口终点")]
        private float _normalizedInterruptionWindowEnd;
        [SerializeField, Min(0f), Tooltip("动画过渡")]
        private float _animationBlendSeconds;
        [SerializeField, Tooltip("角色意图")]
        private CharacterIntention _requiredIntention;
        [SerializeField, Tooltip("角色事实")]
        private CharacterFact _requiredFact;

        public string SourceActionId => _sourceActionId;
        public string TargetActionId => _targetActionId;
        public int Priority => _priority;
        public float NormalizedInterruptionWindowStart => _normalizedInterruptionWindowStart;
        public float NormalizedInterruptionWindowEnd => _normalizedInterruptionWindowEnd;
        public float AnimationBlendSeconds => _animationBlendSeconds;
        public CharacterIntention RequiredIntention => _requiredIntention;
        public CharacterFact RequiredFact => _requiredFact;
    }

    [CreateAssetMenu(fileName = "CharacterActionSet", menuName = "ZZZ/角色/动作资产集合")]
    public sealed class CharacterActionSetAsset : ScriptableObject
    {
        [SerializeField, Tooltip("初始动作资产")]
        private CharacterActionAsset _initialAction;
        [SerializeField, Tooltip("动作资产列表")]
        private List<CharacterActionAsset> _actions = new List<CharacterActionAsset>();
        [SerializeField, Tooltip("动作转移规则列表")]
        private List<CharacterActionLink> _links = new List<CharacterActionLink>();

        public CharacterActionAsset InitialAction => _initialAction;
        public IReadOnlyList<CharacterActionAsset> Actions => _actions;
        public IReadOnlyList<CharacterActionLink> Links => _links;

        /// <summary>
        /// 根据优先级构造字典
        /// </summary>
        public void BuildRuntimeLookups(
            out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
            out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            var builtActionsById = new Dictionary<string, CharacterActionAsset>(_actions.Count);
            var actionOrderById = new Dictionary<string, int>(_actions.Count);
            for (int index = 0; index < _actions.Count; index++)
            {
                CharacterActionAsset action = _actions[index];
                builtActionsById.Add(action.ActionId, action);
                actionOrderById.Add(action.ActionId, index);
            }

            var builtLinksBySourceActionId = new Dictionary<string, List<CharacterActionLink>>();
            for (int index = 0; index < _links.Count; index++)
            {
                CharacterActionLink link = _links[index];
                if (!builtLinksBySourceActionId.TryGetValue(link.SourceActionId, out List<CharacterActionLink> sourceLinks))
                {
                    sourceLinks = new List<CharacterActionLink>();
                    builtLinksBySourceActionId.Add(link.SourceActionId, sourceLinks);
                }

                sourceLinks.Add(link);
            }

            var readonlyLinksBySourceActionId = new Dictionary<string, IReadOnlyList<CharacterActionLink>>(builtLinksBySourceActionId.Count);
            foreach (KeyValuePair<string, List<CharacterActionLink>> pair in builtLinksBySourceActionId)
            {
                List<CharacterActionLink> sourceLinks = pair.Value;
                for (int index = 1; index < sourceLinks.Count; index++)
                {
                    CharacterActionLink link = sourceLinks[index];
                    int previousIndex = index - 1;
                    while (previousIndex >= 0 && IsLowerPriority(sourceLinks[previousIndex], link, actionOrderById))
                    {
                        sourceLinks[previousIndex + 1] = sourceLinks[previousIndex];
                        previousIndex--;
                    }

                    sourceLinks[previousIndex + 1] = link;
                }

                readonlyLinksBySourceActionId.Add(pair.Key, new ReadOnlyCollection<CharacterActionLink>(sourceLinks.ToArray()));
            }

            actionsById = new ReadOnlyDictionary<string, CharacterActionAsset>(builtActionsById);
            linksBySourceActionId = new ReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>>(readonlyLinksBySourceActionId);
        }

        private static bool IsLowerPriority(
            CharacterActionLink previousLink,
            CharacterActionLink currentLink,
            IReadOnlyDictionary<string, int> actionOrderById)
        {
            if (previousLink.Priority != currentLink.Priority)
            {
                return previousLink.Priority < currentLink.Priority;
            }

            return actionOrderById[previousLink.TargetActionId] > actionOrderById[currentLink.TargetActionId];
        }
    }
}
