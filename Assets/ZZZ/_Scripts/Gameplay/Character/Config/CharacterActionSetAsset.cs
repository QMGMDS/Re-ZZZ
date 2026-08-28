using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色动作之间的有向连接及其裁决条件
    /// </summary>
    [Serializable]
    public struct CharacterActionLink
    {
        [SerializeField, Tooltip("出边")]
        private string _fromActionId;
        [SerializeField, Tooltip("去边")]
        private string _toActionId;
        [SerializeField, Range(0f, 1f), Tooltip("打断窗口起点")]
        private float _interruptProgress;
        [SerializeField, Range(0f, 1f), Tooltip("打断窗口终点")]
        private float _interruptEndProgress;
        [SerializeField, Tooltip("优先级")]
        private int _priority;
        [SerializeField, Tooltip("角色本帧意图")]
        private CharacterIntention _requiredIntention;
        [SerializeField, Tooltip("角色本帧所处事实")]
        private CharacterFact _requiredFact;
        [SerializeField, FormerlySerializedAs("_animationTransitionNormalizedDuration"), Min(0f), Tooltip("动画混合过渡时长 单位为秒")]
        private float _animationTransitionDurationSeconds;

        public string FromActionId => _fromActionId;
        public string ToActionId => _toActionId;
        public float InterruptWindowStartProgress => _interruptProgress;
        public float InterruptWindowEndProgress => _interruptEndProgress;
        public int Priority => _priority;
        public CharacterIntention RequiredIntention => _requiredIntention;
        public CharacterFact RequiredFact => _requiredFact;
        public float AnimationTransitionDurationSeconds => _animationTransitionDurationSeconds;
    }

    /// <summary>
    /// 角色可使用的动作资产集合
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterActionSet", menuName = "ZZZ/角色/动作资产集合")]
    public sealed class CharacterActionSetAsset : ScriptableObject
    {
        [SerializeField]
        private CharacterActionAsset _defaultAction;
        [SerializeField]
        private List<CharacterActionAsset> _actions = new List<CharacterActionAsset>();
        [SerializeField]
        private List<CharacterActionLink> _links = new List<CharacterActionLink>();

        public CharacterActionAsset DefaultAction => _defaultAction;

        /// <summary>
        /// 根据动作配置生成运行时查询字典
        /// </summary>
        public void BuildRuntimeLookups(
            out IReadOnlyDictionary<string, CharacterActionAsset> actionsById,
            out IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            if (_actions.Count == 0)
            {
                throw new InvalidOperationException($"[{name}] 动作列表不能为空");
            }

            var builtActionsById = new Dictionary<string, CharacterActionAsset>(_actions.Count);
            var actionOrderById = new Dictionary<string, int>(_actions.Count);

            for (int index = 0; index < _actions.Count; index++)
            {
                CharacterActionAsset action = _actions[index];
                builtActionsById.Add(action.Id, action);
                actionOrderById.Add(action.Id, index);
            }

            var pendingLinks = new Dictionary<string, List<CharacterActionLink>>();
            for (int index = 0; index < _links.Count; index++)
            {
                CharacterActionLink link = _links[index];
                CharacterActionAsset sourceAction = builtActionsById[link.FromActionId];

                if (float.IsNaN(link.InterruptWindowStartProgress)
                    || float.IsInfinity(link.InterruptWindowStartProgress)
                    || link.InterruptWindowStartProgress < 0f
                    || link.InterruptWindowStartProgress > 1f
                    || float.IsNaN(link.InterruptWindowEndProgress)
                    || float.IsInfinity(link.InterruptWindowEndProgress)
                    || link.InterruptWindowEndProgress < 0f
                    || link.InterruptWindowEndProgress > 1f)
                {
                    throw new InvalidOperationException(
                        $"[{name}] 动作链接 {link.FromActionId} -> {link.ToActionId} 的打断窗口必须位于 0 到 1 之间");
                }

                if (link.InterruptWindowStartProgress > link.InterruptWindowEndProgress)
                {
                    throw new InvalidOperationException(
                        $"[{name}] 动作链接 {link.FromActionId} -> {link.ToActionId} 的打断窗口起点不能晚于终点");
                }

                if (float.IsNaN(link.AnimationTransitionDurationSeconds)
                    || float.IsInfinity(link.AnimationTransitionDurationSeconds)
                    || link.AnimationTransitionDurationSeconds < 0f)
                {
                    throw new InvalidOperationException(
                        $"[{name}] 动作链接 {link.FromActionId} -> {link.ToActionId} 的动画混合过渡时长必须是大于等于 0 的有限秒数");
                }

                if (!pendingLinks.TryGetValue(sourceAction.Id, out List<CharacterActionLink> outgoingLinks))
                {
                    outgoingLinks = new List<CharacterActionLink>();
                    pendingLinks.Add(sourceAction.Id, outgoingLinks);
                }

                outgoingLinks.Add(link);
            }

            var builtLinksBySourceActionId =
                new Dictionary<string, IReadOnlyList<CharacterActionLink>>(pendingLinks.Count);
            foreach (var pair in pendingLinks)
            {
                builtLinksBySourceActionId.Add(
                    pair.Key,
                    pair.Value
                        .OrderByDescending(link => link.Priority)
                        .ThenBy(link => actionOrderById[link.ToActionId])
                        .ToArray());
            }

            actionsById = builtActionsById;
            linksBySourceActionId = builtLinksBySourceActionId;
        }
    }
}
