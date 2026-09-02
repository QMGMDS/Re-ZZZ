using System;

using UnityEngine;

namespace GamePlay.Character
{
    public sealed class CharacterAnimationPlayer : IDisposable
    {
        private readonly CharacterPlayableGraph _playableGraph;

        private CharacterActionAsset _currentAction;
        private bool _isInitialized;
        private bool _isDisposed;

        public CharacterAnimationPlayer(Animator animator)
        {
            if (animator == null)
            {
                throw new ArgumentNullException(nameof(animator));
            }

            _playableGraph = new CharacterPlayableGraph(animator, nameof(CharacterAnimationPlayer));
        }

        public CharacterActionAsset CurrentAction
        {
            get
            {
                EnsureNotDisposed();
                return _currentAction;
            }
        }

        public int ActiveClipPlayableCount
        {
            get
            {
                EnsureNotDisposed();
                return _playableGraph.ActiveClipPlayableCount;
            }
        }

        public void Reset(CharacterActionAsset initialAction)
        {
            EnsureNotDisposed();
            EnsureAction(initialAction);

            _playableGraph.BindInitial(initialAction.AnimationClip, 0f);
            _currentAction = initialAction;
            _isInitialized = true;
        }

        public void EnterAction(CharacterActionAsset action, in CharacterActionLink selectedLink)
        {
            EnsureNotDisposed();
            EnsureAction(action);

            if (!_isInitialized || _currentAction == null)
            {
                throw new InvalidOperationException("动画播放器尚未初始化");
            }

            if (!string.Equals(selectedLink.TargetActionId, action.ActionId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"动画链接目标 ID 与进入动作不一致 {selectedLink.TargetActionId}");
            }

            if (!string.Equals(selectedLink.SourceActionId, _currentAction.ActionId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"动画链接源 ID 与当前动作不一致 {selectedLink.SourceActionId}");
            }

            _playableGraph.StartTransition(
                action.AnimationClip,
                0f,
                selectedLink.AnimationBlendSeconds);
            _currentAction = action;
        }

        public void Evaluate(
            CharacterActionAsset currentAction,
            float logicalProgressSeconds,
            float deltaTimeSeconds)
        {
            EnsureNotDisposed();
            EnsureAction(currentAction);

            if (!_isInitialized || _currentAction == null)
            {
                throw new InvalidOperationException("动画播放器尚未初始化");
            }

            if (!ReferenceEquals(_currentAction, currentAction))
            {
                throw new InvalidOperationException("动画播放器当前动作与输入动作不一致");
            }

            if (float.IsNaN(logicalProgressSeconds)
                || float.IsInfinity(logicalProgressSeconds)
                || logicalProgressSeconds < 0f
                || logicalProgressSeconds > currentAction.DurationSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalProgressSeconds));
            }

            if (float.IsNaN(deltaTimeSeconds)
                || float.IsInfinity(deltaTimeSeconds)
                || deltaTimeSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));
            }

            float normalizedProgress = logicalProgressSeconds / currentAction.DurationSeconds;
            float clipTimeSeconds = normalizedProgress * currentAction.AnimationClip.length;
            _playableGraph.SampleCurrent(clipTimeSeconds, deltaTimeSeconds);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _playableGraph.Dispose();
            _currentAction = null;
            _isInitialized = false;
            _isDisposed = true;
        }

        private static void EnsureAction(CharacterActionAsset action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(CharacterAnimationPlayer));
            }
        }
    }
}
