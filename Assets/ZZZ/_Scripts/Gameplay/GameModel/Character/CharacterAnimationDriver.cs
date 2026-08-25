using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

using GamePlay.Data;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 根据角色动作及其逻辑时间手动采样动画
    /// </summary>
    public sealed class CharacterAnimationDriver : IDisposable
    {
        private sealed class AnimationSource
        {
            public AnimationClipPlayable Playable;
            public int InputIndex;
            public float TransitionStartWeight;
        }

        private readonly IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> _linksBySourceActionId;
        private readonly List<AnimationSource> _sources = new List<AnimationSource>();

        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationMixerPlayable _mixer;

        private CharacterActionAsset _currentAction;
        private AnimationSource _currentSource;
        private float _previousLogicalProgressSeconds;
        private float _transitionElapsedSeconds;
        private float _transitionDurationSeconds;
        private bool _isTransitioning;

        public CharacterAnimationDriver(
            Animator animator,
            IReadOnlyDictionary<string, IReadOnlyList<CharacterActionLink>> linksBySourceActionId)
        {
            _linksBySourceActionId = linksBySourceActionId;

            _graph = PlayableGraph.Create(nameof(CharacterAnimationDriver));
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual); // 手动控制

            _output = AnimationPlayableOutput.Create(_graph, "CharacterAnimation", animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 0);
            _output.SetSourcePlayable(_mixer);

            _graph.Play();
        }

        /// <summary>
        /// 将当前动作采样到指定的逻辑秒数，并独立推进旧动画及动画混合
        /// </summary>
        public void Evaluate(CharacterActionAsset currentAction, float logicalProgressSeconds, float deltaTime)
        {
            if (currentAction == null)
            {
                throw new ArgumentNullException(nameof(currentAction));
            }

            if (_currentAction == null)
            {
                BindFirstAction(currentAction);
            }
            else if (_currentAction != currentAction || logicalProgressSeconds < _previousLogicalProgressSeconds)
            {
                BeginTransition(currentAction);
            }

            SetCurrentTime(logicalProgressSeconds);
            AdvanceTransition(deltaTime);

            _graph.Evaluate(deltaTime);
            _previousLogicalProgressSeconds = logicalProgressSeconds;
        }

        private void BindFirstAction(CharacterActionAsset action)
        {
            _currentSource = AddSource(action.AnimationClip, 1f);
            _currentAction = action;
        }

        private void BeginTransition(CharacterActionAsset nextAction)
        {
            for (int index = 0; index < _sources.Count; index++)
            {
                AnimationSource source = _sources[index];
                source.TransitionStartWeight = _mixer.GetInputWeight(source.InputIndex);
            }

            double outgoingSpeed =
                (double)_currentAction.AnimationClip.length / _currentAction.DurationSeconds;
            _currentSource.Playable.SetSpeed(outgoingSpeed);

            float durationSeconds = GetTransitionDurationSeconds(_currentAction.Id, nextAction.Id);

            _currentSource = AddSource(nextAction.AnimationClip, 0f);
            _currentAction = nextAction;
            _transitionElapsedSeconds = 0f;
            _transitionDurationSeconds = durationSeconds;

            if (durationSeconds == 0f)
            {
                CompleteTransition();
                return;
            }

            _isTransitioning = true;
        }

        private AnimationSource AddSource(AnimationClip animationClip, float weight)
        {
            AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, animationClip);
            playable.SetSpeed(0d);

            int inputIndex = _mixer.GetInputCount();
            _mixer.SetInputCount(inputIndex + 1);
            _graph.Connect(playable, 0, _mixer, inputIndex);
            _mixer.SetInputWeight(inputIndex, weight);

            var source = new AnimationSource
            {
                Playable = playable,
                InputIndex = inputIndex,
                TransitionStartWeight = weight
            };
            _sources.Add(source);

            return source;
        }

        private void SetCurrentTime(float logicalProgressSeconds)
        {
            AnimationClip animationClip = _currentAction.AnimationClip;
            double normalizedProgress =
                (double)logicalProgressSeconds / _currentAction.DurationSeconds;
            _currentSource.Playable.SetTime(normalizedProgress * animationClip.length);
        }

        private void AdvanceTransition(float deltaTime)
        {
            if (!_isTransitioning)
            {
                return;
            }

            _transitionElapsedSeconds =
                Mathf.Min(_transitionElapsedSeconds + deltaTime, _transitionDurationSeconds);
            float incomingWeight = _transitionElapsedSeconds / _transitionDurationSeconds;
            float outgoingWeight = 1f - incomingWeight;

            for (int index = 0; index < _sources.Count; index++)
            {
                AnimationSource source = _sources[index];
                float weight = source == _currentSource
                    ? incomingWeight
                    : source.TransitionStartWeight * outgoingWeight;
                _mixer.SetInputWeight(source.InputIndex, weight);
            }

            if (_transitionElapsedSeconds >= _transitionDurationSeconds)
            {
                CompleteTransition();
            }
        }

        private float GetTransitionDurationSeconds(string sourceActionId, string targetActionId)
        {
            float? durationSeconds = null;

            if (_linksBySourceActionId.TryGetValue(
                    sourceActionId,
                    out IReadOnlyList<CharacterActionLink> outgoingLinks))
            {
                for (int index = 0; index < outgoingLinks.Count; index++)
                {
                    CharacterActionLink link = outgoingLinks[index];
                    if (link.ToActionId == targetActionId)
                    {
                        if (durationSeconds.HasValue)
                        {
                            throw new InvalidOperationException(
                                $"动作链接 {sourceActionId} -> {targetActionId} 只能配置一条");
                        }

                        durationSeconds = link.AnimationTransitionDurationSeconds;
                    }
                }
            }

            if (!durationSeconds.HasValue)
            {
                throw new InvalidOperationException(
                    $"未配置动作链接 {sourceActionId} -> {targetActionId} 的动画过渡时长");
            }

            return durationSeconds.Value;
        }

        private void CompleteTransition()
        {
            for (int index = 0; index < _sources.Count; index++)
            {
                AnimationSource source = _sources[index];
                _graph.Disconnect(_mixer, source.InputIndex);

                if (source != _currentSource)
                {
                    _graph.DestroyPlayable(source.Playable);
                }
            }

            _sources.Clear();
            _mixer.SetInputCount(1);
            _graph.Connect(_currentSource.Playable, 0, _mixer, 0);
            _mixer.SetInputWeight(0, 1f);

            _currentSource.InputIndex = 0;
            _currentSource.TransitionStartWeight = 1f;
            _sources.Add(_currentSource);
            _isTransitioning = false;
        }

        public void Dispose()
        {
            _graph.Destroy();
        }
    }
}
