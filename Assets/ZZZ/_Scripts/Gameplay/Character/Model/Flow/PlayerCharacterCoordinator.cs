using System;

namespace GamePlay.Character
{
    public sealed class PlayerCharacterCoordinator : IDisposable
    {
        private readonly CharacterActionArbiter _arbiter;
        private readonly CharacterActionTransition _transition;
        private readonly CharacterPositionDriver _positionDriver;
        private readonly CharacterRotationDriver _rotationDriver;
        private readonly CharacterAnimationPlayer _animationPlayer;

        private bool _hasEnteredCurrentAction;
        private bool _isInitialized;
        private bool _isDisposed;

        public PlayerCharacterCoordinator(
            CharacterActionArbiter arbiter,
            CharacterActionTransition transition,
            CharacterPositionDriver positionDriver,
            CharacterRotationDriver rotationDriver,
            CharacterAnimationPlayer animationPlayer)
        {
            _arbiter = arbiter ?? throw new ArgumentNullException(nameof(arbiter));
            _transition = transition ?? throw new ArgumentNullException(nameof(transition));
            _positionDriver = positionDriver ?? throw new ArgumentNullException(nameof(positionDriver));
            _rotationDriver = rotationDriver ?? throw new ArgumentNullException(nameof(rotationDriver));
            _animationPlayer = animationPlayer ?? throw new ArgumentNullException(nameof(animationPlayer));
        }

        public void Reset(CharacterActionAsset initialAction, ref CharacterActionState state)
        {
            EnsureNotDisposed();
            if (initialAction == null)
            {
                throw new ArgumentNullException(nameof(initialAction));
            }

            CharacterActionAsset configuredInitialAction = _transition.GetAction(initialAction.ActionId);
            if (!ReferenceEquals(configuredInitialAction, initialAction))
            {
                throw new InvalidOperationException("初始动作不是动作集合中的配置实例");
            }

            state.Intention.ValidateRuntime();
            state.Fact.ValidateRuntime();
            state.CurrentActionId = initialAction.ActionId;
            state.LogicalProgressSeconds = 0f;
            state.ActionDirectionInWorld = UnityEngine.Vector2.zero;

            _positionDriver.Reset();
            _animationPlayer.Reset(initialAction);
            _hasEnteredCurrentAction = false;
            _isInitialized = true;
        }

        public void Tick(ref CharacterActionState state, float deltaTime)
        {
            EnsureNotDisposed();
            if (!_isInitialized)
            {
                throw new InvalidOperationException("角色协调器尚未初始化");
            }

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            state.Intention.ValidateRuntime();
            state.Fact.ValidateRuntime();
            CharacterActionAsset currentAction = _transition.GetAction(state.CurrentActionId);
            if (float.IsNaN(state.LogicalProgressSeconds)
                || float.IsInfinity(state.LogicalProgressSeconds)
                || state.LogicalProgressSeconds < 0f
                || state.LogicalProgressSeconds > currentAction.DurationSeconds)
            {
                throw new InvalidOperationException("当前动作逻辑进度无效");
            }

            float normalizedProgress = state.LogicalProgressSeconds / currentAction.DurationSeconds;
            bool hasSelectedLink = _arbiter.TrySelect(
                state.CurrentActionId,
                normalizedProgress,
                state.Intention,
                state.Fact,
                out CharacterActionLink selectedLink,
                out _);

            bool actionEntered = false;
            if (hasSelectedLink)
            {
                currentAction = _transition.ApplySelectedLink(ref state, selectedLink);
                actionEntered = true;
            }
            else if (!_hasEnteredCurrentAction)
            {
                actionEntered = true;
            }

            _rotationDriver.UpdateActionDirection(ref state, currentAction, actionEntered);
            _transition.Advance(ref state, deltaTime);
            _positionDriver.Evaluate(state, currentAction, actionEntered);
            _rotationDriver.ApplyRotation(state, currentAction, actionEntered, deltaTime);

            if (hasSelectedLink)
            {
                _animationPlayer.EnterAction(currentAction, selectedLink);
            }

            _animationPlayer.Evaluate(currentAction, state.LogicalProgressSeconds, deltaTime);
            _hasEnteredCurrentAction = true;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _animationPlayer.Dispose();
            _isInitialized = false;
            _isDisposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(PlayerCharacterCoordinator));
            }
        }
    }
}
