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

        public void AnimationPlay(ref CharacterActionState state)
        {
            CharacterActionAsset currentAction = _actionsById[state.CurrentActionId];

            // 逻辑时长转动画时长
            float normalizedProgress = state.LogicalProgressSeconds / currentAction.DurationSeconds;
            float clipTimeSeconds = normalizedProgress * currentAction.AnimationClip.length;

            if (!_currentPlayable.IsValid() || !string.Equals(_currentActionId, state.CurrentActionId, StringComparison.Ordinal))
            {
                Play(currentAction.AnimationClip, clipTimeSeconds);
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
            _currentActionId = null;
        }

        private void Play(AnimationClip clip, float clipTimeSeconds)
        {
            AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, clip);
            playable.SetSpeed(0d);
            playable.SetTime(clipTimeSeconds);
            _output.SetSourcePlayable(playable);

            if (_currentPlayable.IsValid())
            {
                _graph.DestroyPlayable(_currentPlayable);
            }

            _currentPlayable = playable;
            _graph.Evaluate(0f);
        }

        private void Sample(float clipTimeSeconds)
        {
            _currentPlayable.SetTime(clipTimeSeconds);
            _graph.Evaluate(0f);
        }
    }
}
