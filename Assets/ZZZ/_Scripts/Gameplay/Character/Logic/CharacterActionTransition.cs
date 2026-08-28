using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 处理动作切换及其逻辑时间推进
    /// </summary>
    public sealed class CharacterActionTransition
    {
        private float _elapsedSeconds;

        /// <summary>
        /// 读取当前动作的归一化逻辑进度
        /// </summary>
        public float GetNormalizedProgress(CharacterActionAsset currentAction)
        {
            return Mathf.Clamp01(_elapsedSeconds / currentAction.DurationSeconds);
        }

        /// <summary>
        /// 处理裁决结果并推进当前动作，返回已推进的逻辑秒数
        /// </summary>
        public float Tick(
            CharacterActionAsset targetAction,
            ref CharacterActionAsset currentAction,
            float deltaTime)
        {
            if (targetAction != null && (currentAction != targetAction || _elapsedSeconds >= currentAction.DurationSeconds))
            {
                StartAction(targetAction, ref currentAction);
            }

            if (currentAction == null)
            {
                return 0f;
            }

            _elapsedSeconds = Mathf.Min(_elapsedSeconds + deltaTime, currentAction.DurationSeconds);

            return _elapsedSeconds;
        }

        private void StartAction(
            CharacterActionAsset action,
            ref CharacterActionAsset currentAction)
        {
            currentAction = action;
            _elapsedSeconds = 0f;
        }
    }
}
