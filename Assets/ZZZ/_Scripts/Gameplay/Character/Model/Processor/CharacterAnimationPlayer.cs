using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GamePlay.Character
{
    public sealed class CharacterAnimationPlayer
    {
        private readonly IReadOnlyDictionary<string, CharacterActionAsset> _actionsById;

        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;

        // 当前使用的动画节点
        private AnimationClipPlayable _currentPlayable;

        // 当前可视动画节点 可能是动画节点或过渡混合中的节点
        private Playable _currentVisualPlayable;

        // 当前过渡混合节点
        private AnimationMixerPlayable _blendMixer;
        private float _blendDurationSeconds;
        private float _blendElapsedSeconds;

        // 当前所处的动作ID，根据此ID切换动画
        private string _currentActionId;

        public CharacterAnimationPlayer(IReadOnlyDictionary<string, CharacterActionAsset> actionsById, Animator animator)
        {
            _actionsById = actionsById;

            _graph = PlayableGraph.Create(nameof(CharacterAnimationPlayer));
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            _output = AnimationPlayableOutput.Create(_graph, "CharacterAnimation", animator);
            _graph.Play();
        }

        public void AnimationPlay(ref CharacterActionState state, float animationBlendSeconds)
        {
            CharacterActionAsset currentAction = _actionsById[state.CurrentActionId];

            // 逻辑时长转动画时长
            float normalizedProgress = state.LogicalProgressSeconds / currentAction.DurationSeconds;
            float clipTimeSeconds = normalizedProgress * currentAction.AnimationClip.length;

            if (!_currentPlayable.IsValid() || !string.Equals(_currentActionId, state.CurrentActionId, StringComparison.Ordinal))
            {
                Play(currentAction.AnimationClip, clipTimeSeconds, animationBlendSeconds);
                _currentActionId = state.CurrentActionId;
                return;
            }

            Sample(clipTimeSeconds);
        }

        public void Dispose()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            _graph = default;
            _output = default;
            _currentPlayable = default;
            _currentVisualPlayable = default;
            _blendMixer = default;
            _blendDurationSeconds = 0f;
            _blendElapsedSeconds = 0f;
            _currentActionId = null;
        }

        /// <summary>
        /// 播放新动作
        /// </summary>
        private void Play(AnimationClip clip, float clipTimeSeconds, float animationBlendSeconds)
        {
            // AnimationClip 转计算节点
            AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, clip);
            playable.SetSpeed(0d);
            playable.SetTime(clipTimeSeconds);

            if (animationBlendSeconds > 0f && _currentVisualPlayable.IsValid())
            {
                StartBlend(playable, animationBlendSeconds);
            }
            else
            {
                _output.SetSourcePlayable(playable);
                DestroyCurrentVisualPlayable();
                _currentVisualPlayable = playable;
                ResetBlend();
            }

            _currentPlayable = playable;
            _graph.Evaluate(0f);
        }

        /// <summary>
        /// 播放动画所处时刻
        /// </summary>
        private void Sample(float clipTimeSeconds)
        {
            _currentPlayable.SetTime(clipTimeSeconds);

            if (_blendMixer.IsValid())
            {
                _blendElapsedSeconds += Time.deltaTime;
                float blendProgress = Mathf.Clamp01(_blendElapsedSeconds / _blendDurationSeconds);
                _blendMixer.SetInputWeight(0, 1f - blendProgress);
                _blendMixer.SetInputWeight(1, blendProgress);

                if (blendProgress >= 1f)
                {
                    CompleteBlend();
                }
            }

            _graph.Evaluate(0f);
        }

        private void StartBlend(AnimationClipPlayable playable, float blendDurationSeconds)
        {
            AnimationMixerPlayable blendMixer = AnimationMixerPlayable.Create(_graph, 2);
            _output.SetSourcePlayable(blendMixer);
            _graph.Connect(_currentVisualPlayable, 0, blendMixer, 0);
            _graph.Connect(playable, 0, blendMixer, 1);
            blendMixer.SetInputWeight(0, 1f);
            blendMixer.SetInputWeight(1, 0f);

            _currentVisualPlayable = blendMixer;
            _blendMixer = blendMixer;
            _blendDurationSeconds = blendDurationSeconds;
            _blendElapsedSeconds = 0f;
        }

        private void CompleteBlend()
        {
            AnimationMixerPlayable completedBlendMixer = _blendMixer;
            _graph.Disconnect(completedBlendMixer, 1);
            _output.SetSourcePlayable(_currentPlayable);
            _graph.DestroySubgraph(completedBlendMixer);

            _currentVisualPlayable = _currentPlayable;
            ResetBlend();
        }

        private void DestroyCurrentVisualPlayable()
        {
            if (_currentVisualPlayable.IsValid())
            {
                _graph.DestroySubgraph(_currentVisualPlayable);
            }
        }

        private void ResetBlend()
        {
            _blendMixer = default;
            _blendDurationSeconds = 0f;
            _blendElapsedSeconds = 0f;
        }
    }
}
