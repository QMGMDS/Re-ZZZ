using UnityEngine;

namespace GamePlay.Character
{
    /// <summary>
    /// 角色动作逻辑模型在一次 Tick 后的最新状态
    /// </summary>
    public readonly struct CharacterActionState
    {
        public CharacterActionAsset CurrentAction { get; }
        public float LogicalProgressSeconds { get; }
        public Vector3 WorldDirection { get; }
        public CharacterActionDirectionMode DirectionMode { get; }
        public bool ActionStarted { get; }
        public bool DirectionStarted { get; }

        public CharacterActionState(
            CharacterActionAsset currentAction,
            float logicalProgressSeconds,
            Vector3 worldDirection,
            CharacterActionDirectionMode directionMode,
            bool actionStarted,
            bool directionStarted)
        {
            CurrentAction = currentAction;
            LogicalProgressSeconds = logicalProgressSeconds;
            WorldDirection = worldDirection;
            DirectionMode = directionMode;
            ActionStarted = actionStarted;
            DirectionStarted = directionStarted;
        }
    }
}
