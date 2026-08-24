using UnityEngine;

namespace GamePlay.Data
{
    /// <summary>
    /// 单个角色动作的静态配置
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterAction", menuName = "ZZZ/角色/动作单资产")]
    public sealed class CharacterActionAsset : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField, Min(0f)] private float _durationSeconds;
        [SerializeField] private AnimationClip _animationClip;

        public string Id => _id;
        public float DurationSeconds => _durationSeconds;
        public AnimationClip AnimationClip => _animationClip;
    }
}
