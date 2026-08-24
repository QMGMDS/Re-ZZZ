using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 角色可使用的动作资产集合
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterActionSet", menuName = "ZZZ/角色/动作资产集合")]
    public sealed class CharacterActionSetAsset : ScriptableObject
    {
        [SerializeField] private List<CharacterActionAsset> _actions = new List<CharacterActionAsset>();

        public IReadOnlyList<CharacterActionAsset> Actions => _actions;

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

            var priorityGroups = validActions.GroupBy(a => a.Priority).Where(g => g.Count() > 1).ToList();
            if (priorityGroups.Any())
            {
                var duplicatePriorities = string.Join(", ", priorityGroups.Select(g => g.Key));
                Debug.LogWarning($"[{name}] 存在重复的优先级：{duplicatePriorities}，可能影响选择顺序", this);
            }

            var intentionFactGroups = validActions
                .GroupBy(a => (a.RequiredIntention, a.RequiredFact))
                .Where(g => g.Count() > 1)
                .ToList();
            foreach (var group in intentionFactGroups)
            {
                var pairs = string.Join(", ", group.Select(a => $"{a.Id}(P:{a.Priority})"));
                Debug.LogWarning($"[{name}] 存在多动作响应同一 (Intention, Fact)：{group.Key}，请确认优先级是否明确。动作：{pairs}", this);
            }
        }
#endif

    }
}
