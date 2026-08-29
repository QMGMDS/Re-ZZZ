using System;

using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 处理动作切换及其逻辑时间推进
    /// </summary>
    public sealed class CharacterActionTransition
    {
        private CharacterActionAsset _currentAction;
        private float _elapsedSeconds;

        public CharacterActionAsset CurrentAction => _currentAction;
        public float LogicalProgressSeconds => _elapsedSeconds;

        public CharacterActionTransition(CharacterActionAsset defaultAction)
        {
            if (defaultAction == null)
            {
                throw new ArgumentNullException(nameof(defaultAction));
            }

            _currentAction = defaultAction;
        }

        /// <summary>
        /// 读取当前动作的归一化逻辑进度
        /// </summary>
        public float GetNormalizedProgress()
        {
            return Mathf.Clamp01(_elapsedSeconds / _currentAction.DurationSeconds);
        }

        /// <summary>
        /// 处理裁决结果并推进当前动作，返回已推进的逻辑秒数
        /// </summary>
        public float Tick(
            CharacterActionAsset targetAction,
            float deltaTime,
            bool restartCurrentAction,
            out bool actionStarted)
        {
            actionStarted = false;

            if (targetAction != null
                && (restartCurrentAction
                    || _currentAction != targetAction
                    || _elapsedSeconds >= _currentAction.DurationSeconds))
            {
                StartAction(targetAction);
                actionStarted = true;
            }

            _elapsedSeconds = Mathf.Min(_elapsedSeconds + deltaTime, _currentAction.DurationSeconds);

            return _elapsedSeconds;
        }

        private void StartAction(
            CharacterActionAsset action)
        {
            _currentAction = action;
            _elapsedSeconds = 0f;
        }
    }
}
