using System;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GamePlay.Character
{
    public sealed class CharacterPlayableGraph : IDisposable
    {
        private readonly Animator _animator;

        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _currentPlayable;
        private AnimationClipPlayable _outgoingPlayable;
        private AnimationClip _currentClip;
        private float _outgoingClipLength;
        private float _currentClipTimeSeconds;
        private float _outgoingClipTimeSeconds;
        private float _blendElapsedSeconds;
        private float _blendDurationSeconds;
        private bool _hasCurrentPlayable;
        private bool _isBlending;
        private bool _isDisposed;

        public CharacterPlayableGraph(Animator animator)
            : this(animator, nameof(CharacterPlayableGraph))
        {
        }

        public CharacterPlayableGraph(Animator animator, string graphName)
        {
            if (animator == null)
            {
                throw new ArgumentNullException(nameof(animator));
            }

            if (string.IsNullOrWhiteSpace(graphName))
            {
                throw new ArgumentException("动画图名称不能为空", nameof(graphName));
            }

            _animator = animator;
            _graph = PlayableGraph.Create(graphName);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            _output = AnimationPlayableOutput.Create(_graph, "CharacterAnimation", _animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 2);
            _output.SetSourcePlayable(_mixer);
            _graph.Play();
        }

        public AnimationClip CurrentClip
        {
            get
            {
                EnsureNotDisposed();
                return _currentClip;
            }
        }

        public bool IsBlending
        {
            get
            {
                EnsureNotDisposed();
                return _isBlending;
            }
        }

        public int ActiveClipPlayableCount
        {
            get
            {
                EnsureNotDisposed();
                return (_hasCurrentPlayable ? 1 : 0) + (_isBlending ? 1 : 0);
            }
        }

        public int InputCount
        {
            get
            {
                EnsureNotDisposed();
                return _mixer.GetInputCount();
            }
        }

        public float CurrentClipTimeSeconds
        {
            get
            {
                EnsureNotDisposed();
                return _currentClipTimeSeconds;
            }
        }

        public void BindInitial(AnimationClip clip, float clipTimeSeconds)
        {
            EnsureNotDisposed();
            ValidateClip(clip);
            ValidateTime(clipTimeSeconds, nameof(clipTimeSeconds));

            ClearPlayableSlots();
            _currentPlayable = CreatePlayable(clip, 0);
            _mixer.SetInputWeight(0, 1f);
            _currentClip = clip;
            _currentClipTimeSeconds = clipTimeSeconds;
            _currentPlayable.SetTime(clipTimeSeconds);
            _hasCurrentPlayable = true;
        }

        public void StartTransition(AnimationClip clip, float clipTimeSeconds, float blendSeconds)
        {
            EnsureNotDisposed();
            ValidateClip(clip);
            ValidateTime(clipTimeSeconds, nameof(clipTimeSeconds));
            ValidateBlendSeconds(blendSeconds);

            if (!_hasCurrentPlayable)
            {
                throw new InvalidOperationException("动画图尚未绑定初始动作");
            }

            if (_isBlending)
            {
                CompleteTransition();
            }

            _outgoingPlayable = _currentPlayable;
            _outgoingClipLength = _currentClip.length;
            _outgoingClipTimeSeconds = _currentClipTimeSeconds;
            _currentPlayable = CreatePlayable(clip, 1);
            _currentPlayable.SetTime(clipTimeSeconds);
            _currentClip = clip;
            _currentClipTimeSeconds = clipTimeSeconds;
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);
            _blendElapsedSeconds = 0f;
            _blendDurationSeconds = blendSeconds;
            _isBlending = true;

            if (blendSeconds == 0f)
            {
                CompleteTransition();
                return;
            }
        }

        public void SampleCurrent(float clipTimeSeconds, float deltaTimeSeconds)
        {
            EnsureNotDisposed();
            if (!_hasCurrentPlayable)
            {
                throw new InvalidOperationException("动画图尚未绑定初始动作");
            }

            ValidateTime(clipTimeSeconds, nameof(clipTimeSeconds));
            ValidateDeltaTime(deltaTimeSeconds);

            _currentClipTimeSeconds = clipTimeSeconds;
            _currentPlayable.SetTime(clipTimeSeconds);

            if (_isBlending)
            {
                _outgoingClipTimeSeconds = Mathf.Min(
                    _outgoingClipTimeSeconds + deltaTimeSeconds,
                    _outgoingClipLength);
                _outgoingPlayable.SetTime(_outgoingClipTimeSeconds);

                _blendElapsedSeconds = Mathf.Min(
                    _blendElapsedSeconds + deltaTimeSeconds,
                    _blendDurationSeconds);
                float incomingWeight = _blendElapsedSeconds / _blendDurationSeconds;
                _mixer.SetInputWeight(0, 1f - incomingWeight);
                _mixer.SetInputWeight(1, incomingWeight);
            }

            _graph.Evaluate(deltaTimeSeconds);

            if (_isBlending && _blendElapsedSeconds >= _blendDurationSeconds)
            {
                CompleteTransition();
            }
        }

        public void CompleteTransition()
        {
            EnsureNotDisposed();
            if (!_isBlending)
            {
                return;
            }

            DisconnectInput(0);
            DisconnectInput(1);
            if (_outgoingPlayable.IsValid())
            {
                _graph.DestroyPlayable(_outgoingPlayable);
            }

            _graph.Connect(_currentPlayable, 0, _mixer, 0);
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);
            _outgoingPlayable = default;
            _outgoingClipLength = 0f;
            _outgoingClipTimeSeconds = 0f;
            _blendElapsedSeconds = 0f;
            _blendDurationSeconds = 0f;
            _isBlending = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            _isDisposed = true;
            _hasCurrentPlayable = false;
            _isBlending = false;
            _currentClip = null;
        }

        private AnimationClipPlayable CreatePlayable(AnimationClip clip, int inputIndex)
        {
            DisconnectInput(inputIndex);
            AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, clip);
            playable.SetSpeed(0d);
            _graph.Connect(playable, 0, _mixer, inputIndex);
            _mixer.SetInputWeight(inputIndex, 0f);
            return playable;
        }

        private void ClearPlayableSlots()
        {
            DisconnectInput(0);
            DisconnectInput(1);

            if (_currentPlayable.IsValid())
            {
                _graph.DestroyPlayable(_currentPlayable);
            }

            if (_outgoingPlayable.IsValid())
            {
                _graph.DestroyPlayable(_outgoingPlayable);
            }

            _currentPlayable = default;
            _outgoingPlayable = default;
            _currentClip = null;
            _currentClipTimeSeconds = 0f;
            _outgoingClipLength = 0f;
            _outgoingClipTimeSeconds = 0f;
            _blendElapsedSeconds = 0f;
            _blendDurationSeconds = 0f;
            _hasCurrentPlayable = false;
            _isBlending = false;
            _mixer.SetInputWeight(0, 0f);
            _mixer.SetInputWeight(1, 0f);
        }

        private void DisconnectInput(int inputIndex)
        {
            if (_mixer.GetInput(inputIndex).IsValid())
            {
                _graph.Disconnect(_mixer, inputIndex);
            }
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(CharacterPlayableGraph));
            }
        }

        private static void ValidateClip(AnimationClip clip)
        {
            if (clip == null)
            {
                throw new ArgumentNullException(nameof(clip));
            }
        }

        private static void ValidateTime(float timeSeconds, string parameterName)
        {
            if (float.IsNaN(timeSeconds) || float.IsInfinity(timeSeconds) || timeSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateDeltaTime(float deltaTimeSeconds)
        {
            if (float.IsNaN(deltaTimeSeconds)
                || float.IsInfinity(deltaTimeSeconds)
                || deltaTimeSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));
            }
        }

        private static void ValidateBlendSeconds(float blendSeconds)
        {
            if (float.IsNaN(blendSeconds) || float.IsInfinity(blendSeconds) || blendSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(blendSeconds));
            }
        }
    }
}
