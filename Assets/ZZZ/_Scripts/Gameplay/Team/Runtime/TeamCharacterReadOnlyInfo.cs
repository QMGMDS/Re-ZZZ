namespace GamePlay.Team
{
    /// <summary>
    /// 队伍事件使用的角色只读值信息
    /// </summary>
    public readonly struct TeamCharacterReadOnlyInfo
    {
        /// <summary>
        /// 角色实体编号
        /// </summary>
        public int EntityId { get; }

        /// <summary>
        /// 角色在队伍中的序号
        /// </summary>
        public int TeamIndex { get; }

        /// <summary>
        /// 当前是否为队伍当前角色
        /// </summary>
        public bool IsCurrentCharacter { get; }

        internal TeamCharacterReadOnlyInfo(
            int entityId,
            int teamIndex,
            bool isCurrentCharacter)
        {
            EntityId = entityId;
            TeamIndex = teamIndex;
            IsCurrentCharacter = isCurrentCharacter;
        }
    }
}
