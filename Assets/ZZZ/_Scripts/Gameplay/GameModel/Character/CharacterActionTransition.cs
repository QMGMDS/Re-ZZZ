using UnityEngine;

using GamePlay.Data;


namespace GamePlay.GameModule
{
    /// <summary>
    /// 处理动作切换及其逻辑时间推进
    /// </summary>
    public sealed class CharacterActionTransition
    {
        private float _elapsedSeconds;

        /// <summary>
        /// 处理裁决结果并推进当前动作，返回已推进的逻辑秒数
        /// </summary>
        public float Tick(
            CharacterActionAsset targetAction,
            ref CharacterActionAsset currentAction,
            float deltaTime,
            ref CharacterFact fact)
        {
            if (targetAction != null && (currentAction != targetAction || _elapsedSeconds >= currentAction.DurationSeconds))
            {
                StartAction(targetAction, ref currentAction, ref fact);
            }

            if (currentAction == null)
            {
                return 0f;
            }

            _elapsedSeconds = Mathf.Min(_elapsedSeconds + deltaTime, currentAction.DurationSeconds);
            fact.SetLogicalProgress(_elapsedSeconds >= currentAction.DurationSeconds ? Trilean.True : Trilean.False);

            return _elapsedSeconds;
        }

        private void StartAction(
            CharacterActionAsset action,
            ref CharacterActionAsset currentAction,
            ref CharacterFact fact)
        {
            currentAction = action;
            _elapsedSeconds = 0f;
            fact.SetLogicalProgress(Trilean.False);
        }
    }
}
