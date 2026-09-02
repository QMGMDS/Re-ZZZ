using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

namespace GamePlay.Character
{
    [Serializable]
    public struct CharacterActionLink
    {
        [SerializeField]
        private string _sourceActionId;
        [SerializeField]
        private string _targetActionId;
        [SerializeField, Range(0f, 1f)]
        private float _normalizedInterruptionWindowStart;
        [SerializeField, Range(0f, 1f)]
        private float _normalizedInterruptionWindowEnd;
        [SerializeField]
        private int _priority;
        [SerializeField]
        private CharacterIntention _requiredIntention;
        [SerializeField]
        private CharacterFact _requiredFact;
        [SerializeField, Min(0f)]
        private float _animationBlendSeconds;

        public string SourceActionId => _sourceActionId;

        public string TargetActionId => _targetActionId;

        public float NormalizedInterruptionWindowStart => _normalizedInterruptionWindowStart;

        public float NormalizedInterruptionWindowEnd => _normalizedInterruptionWindowEnd;

        public int Priority => _priority;

        public CharacterIntention RequiredIntention => _requiredIntention;

        public CharacterFact RequiredFact => _requiredFact;

        public float AnimationBlendSeconds => _animationBlendSeconds;

        public CharacterActionLink(
            string sourceActionId,
            string targetActionId,
            float normalizedInterruptionWindowStart,
            float normalizedInterruptionWindowEnd,
            int priority,
            CharacterIntention requiredIntention,
            CharacterFact requiredFact,
            float animationBlendSeconds)
        {
            if (string.IsNullOrWhiteSpace(sourceActionId))
            {
                throw new ArgumentException("动作链接源 ID 不能为空", nameof(sourceActionId));
            }

            if (string.IsNullOrWhiteSpace(targetActionId))
            {
                throw new ArgumentException("动作链接目标 ID 不能为空", nameof(targetActionId));
            }

            if (!IsNormalizedWindowValue(normalizedInterruptionWindowStart)
                || !IsNormalizedWindowValue(normalizedInterruptionWindowEnd)
                || normalizedInterruptionWindowStart > normalizedInterruptionWindowEnd)
            {
                throw new ArgumentException("动作链接打断窗口必须位于零到一且起点不晚于终点", nameof(normalizedInterruptionWindowStart));
            }

            if (float.IsNaN(animationBlendSeconds)
                || float.IsInfinity(animationBlendSeconds)
                || animationBlendSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(animationBlendSeconds));
            }

            requiredIntention.ValidateCondition();
            requiredFact.ValidateCondition();

            _sourceActionId = sourceActionId;
            _targetActionId = targetActionId;
            _normalizedInterruptionWindowStart = normalizedInterruptionWindowStart;
            _normalizedInterruptionWindowEnd = normalizedInterruptionWindowEnd;
            _priority = priority;
            _requiredIntention = requiredIntention;
            _requiredFact = requiredFact;
            _animationBlendSeconds = animationBlendSeconds;
        }

        private static bool IsNormalizedWindowValue(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 1f;
        }
    }

    [CreateAssetMenu(fileName = "CharacterActionSet", menuName = "ZZZ/角色/动作资产集合")]
    public sealed class CharacterActionSetAsset : ScriptableObject
    {
        [SerializeField]
        private CharacterActionAsset _initialAction;
        [SerializeField]
        private List<CharacterActionAsset> _actions = new List<CharacterActionAsset>();
        [SerializeField]
        private List<CharacterActionLink> _links = new List<CharacterActionLink>();

        public CharacterActionAsset InitialAction => _initialAction;

        public IReadOnlyList<CharacterActionAsset> Actions => _actions;

        public IReadOnlyList<CharacterActionLink> Links => _links;

        public void Validate()
        {
            if (_initialAction == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionSetAsset)} 的 {nameof(InitialAction)} 不能为空");
            }

            _initialAction.Validate();

            if (_actions == null || _actions.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionSetAsset)} 的动作列表不能为空");
            }

            if (_links == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionSetAsset)} 的动作链接列表不能为空");
            }

            var actionsById = new Dictionary<string, CharacterActionAsset>(_actions.Count);
            for (int index = 0; index < _actions.Count; index++)
            {
                CharacterActionAsset action = _actions[index];
                if (action == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(CharacterActionSetAsset)} 的动作列表第 {index} 项不能为空");
                }

                action.Validate();
                if (!actionsById.TryAdd(action.ActionId, action))
                {
                    throw new InvalidOperationException(
                        $"{nameof(CharacterActionSetAsset)} 中动作 ID 重复 {action.ActionId}");
                }
            }

            if (!actionsById.TryGetValue(_initialAction.ActionId, out CharacterActionAsset initialAction)
                || !ReferenceEquals(initialAction, _initialAction))
            {
                throw new InvalidOperationException(
                    $"{nameof(CharacterActionSetAsset)} 的初始动作必须包含在动作列表中");
            }

            for (int index = 0; index < _links.Count; index++)
            {
                CharacterActionLink link = _links[index];
                link.RequiredIntention.ValidateCondition();
                link.RequiredFact.ValidateCondition();

                if (string.IsNullOrWhiteSpace(link.SourceActionId)
                    || string.IsNullOrWhiteSpace(link.TargetActionId))
                {
                    throw new InvalidOperationException(
                        $"{nameof(CharacterActionSetAsset)} 的动作链接端点 ID 不能为空");
                }

                if (!actionsById.ContainsKey(link.SourceActionId)
                    || !actionsById.ContainsKey(link.TargetActionId))
                {
                    throw new InvalidOperationException(
                        $"{nameof(CharacterActionSetAsset)} 的动作链接端点必须存在于动作列表");
                }

                if (!IsNormalizedWindowValue(link.NormalizedInterruptionWindowStart)
                    || !IsNormalizedWindowValue(link.NormalizedInterruptionWindowEnd)
                    || link.NormalizedInterruptionWindowStart > link.NormalizedInterruptionWindowEnd)
                {
                    throw new InvalidOperationException(
                        $"{nameof(CharacterActionSetAsset)} 的动作链接打断窗口必须位于零到一且起点不晚于终点");
                }

                if (float.IsNaN(link.AnimationBlendSeconds)
                    || float.IsInfinity(link.AnimationBlendSeconds)
                    || link.AnimationBlendSeconds < 0f)
                {
                    throw new InvalidOperationException(
                        $"{nameof(CharacterActionSetAsset)} 的动作链接混合秒数必须是大于等于零的有限秒数");
                }
            }
        }

        public void BuildRuntimeLookups(
            out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
            out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            Validate();

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
                if (!builtLinksBySourceActionId.TryGetValue(
                        link.SourceActionId,
                        out List<CharacterActionLink> sourceLinks))
                {
                    sourceLinks = new List<CharacterActionLink>();
                    builtLinksBySourceActionId.Add(link.SourceActionId, sourceLinks);
                }

                sourceLinks.Add(link);
            }

            var readonlyLinksBySourceActionId =
                new Dictionary<string, IReadOnlyList<CharacterActionLink>>(builtLinksBySourceActionId.Count);
            foreach (KeyValuePair<string, List<CharacterActionLink>> pair in builtLinksBySourceActionId)
            {
                List<CharacterActionLink> sourceLinks = pair.Value;
                for (int index = 1; index < sourceLinks.Count; index++)
                {
                    CharacterActionLink link = sourceLinks[index];
                    int previousIndex = index - 1;
                    while (previousIndex >= 0
                        && IsLowerPriority(
                            sourceLinks[previousIndex],
                            link,
                            actionOrderById))
                    {
                        sourceLinks[previousIndex + 1] = sourceLinks[previousIndex];
                        previousIndex--;
                    }

                    sourceLinks[previousIndex + 1] = link;
                }

                readonlyLinksBySourceActionId.Add(
                    pair.Key,
                    new ReadOnlyCollection<CharacterActionLink>(sourceLinks.ToArray()));
            }

            actionsById = new ReadOnlyDictionary<string, CharacterActionAsset>(builtActionsById);
            linksBySourceActionId =
                new ReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>>(
                    readonlyLinksBySourceActionId);
        }

        private static bool IsNormalizedWindowValue(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 1f;
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

            return actionOrderById[previousLink.TargetActionId]
                > actionOrderById[currentLink.TargetActionId];
        }
    }
}
