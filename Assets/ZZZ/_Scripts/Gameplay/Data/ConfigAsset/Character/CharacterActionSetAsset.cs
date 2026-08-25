using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 角色动作之间的有向连接及其裁决条件
    /// </summary>
    [Serializable]
    public struct CharacterActionLink
    {
        [SerializeField] private string _fromActionId;
        [SerializeField] private string _toActionId;
        [SerializeField] private int _priority;
        [SerializeField] private CharacterIntention _requiredIntention;
        [SerializeField] private CharacterFact _requiredFact;
        [SerializeField, Min(0f), Tooltip("切换到目标动作时的线性动画混合时长（秒）")]
        private float _animationTransitionDurationSeconds;

        public string FromActionId => _fromActionId;
        public string ToActionId => _toActionId;
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
        [SerializeField] private List<CharacterActionAsset> _actions = new List<CharacterActionAsset>();
        [SerializeField] private List<CharacterActionLink> _links = new List<CharacterActionLink>();

        public IReadOnlyList<CharacterActionAsset> Actions => _actions;

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

                if (link.AnimationTransitionDurationSeconds < 0f)
                {
                    throw new InvalidOperationException(
                        $"[{name}] 动作链接 {link.FromActionId} -> {link.ToActionId} 的动画过渡时长不能小于 0");
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

#if UNITY_EDITOR
        // 右键菜单手动校验
        [ContextMenu("配置检验是否合法")]
        public void Validate()
        {
            if (_actions == null) return;

            for (int i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i];
                if (action == null)
                {
                    Debug.LogError($"[{name}] 动作列表第 {i} 项为空引用", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(action.Id))
                    Debug.LogError($"[{action.name}] 的 Id 为空", action);

                if (action.DurationSeconds <= 0f)
                    Debug.LogError($"[{action.name}] 的 DurationSeconds 必须大于 0", action);

                if (action.AnimationClip == null)
                    Debug.LogError($"[{action.name}] 未指定 AnimationClip", action);
            }

            var validActions = _actions.Where(a => a != null).ToList();
            var idGroups = validActions.GroupBy(a => a.Id).Where(g => g.Count() > 1).ToList();
            if (idGroups.Any())
            {
                var duplicateIds = string.Join(", ", idGroups.Select(g => g.Key));
                Debug.LogError($"[{name}] 存在重复的 Id：{duplicateIds}", this);
            }

            var validActionIds = new HashSet<string>(validActions
                .Where(action => !string.IsNullOrWhiteSpace(action.Id))
                .GroupBy(action => action.Id)
                .Where(group => group.Count() == 1)
                .Select(group => group.Key));

            if (_links == null) return;

            for (int i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                if (string.IsNullOrWhiteSpace(link.FromActionId))
                {
                    Debug.LogError($"[{name}] 动作链接第 {i} 项的来源动作 Id 为空", this);
                }
                else if (!validActionIds.Contains(link.FromActionId))
                {
                    Debug.LogError($"[{name}] 动作链接第 {i} 项的来源动作 Id 不存在或不唯一：{link.FromActionId}", this);
                }

                if (string.IsNullOrWhiteSpace(link.ToActionId))
                {
                    Debug.LogError($"[{name}] 动作链接第 {i} 项的目标动作 Id 为空", this);
                }
                else if (!validActionIds.Contains(link.ToActionId))
                {
                    Debug.LogError($"[{name}] 动作链接第 {i} 项的目标动作 Id 不存在或不唯一：{link.ToActionId}", this);
                }

                if (link.AnimationTransitionDurationSeconds < 0f)
                {
                    Debug.LogError($"[{name}] 动作链接第 {i} 项的动画过渡时长不能小于 0", this);
                }
            }

            var duplicateLinkGroups = _links
                .GroupBy(link => new { link.FromActionId, link.ToActionId })
                .Where(group => group.Count() > 1);
            foreach (var group in duplicateLinkGroups)
            {
                Debug.LogError(
                    $"[{name}] 动作链接 {group.Key.FromActionId} -> {group.Key.ToActionId} 只能配置一条",
                    this);
            }
        }
#endif

    }
}
