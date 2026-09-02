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
        private AnimationClipPlayable _currentPlayable;
        private string _currentActionId;
        private bool _hasCurrentPlayable;

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

            float normalizedProgress = state.LogicalProgressSeconds / currentAction.DurationSeconds;
            float clipTimeSeconds = normalizedProgress * currentAction.AnimationClip.length;

            if (!_hasCurrentPlayable || !string.Equals(_currentActionId, state.CurrentActionId, StringComparison.Ordinal))
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
            _hasCurrentPlayable = false;
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
            _hasCurrentPlayable = true;
            _graph.Evaluate(0f);
        }

        private void Sample(float clipTimeSeconds)
        {
            _currentPlayable.SetTime(clipTimeSeconds);
            _graph.Evaluate(0f);
        }
    }
}
