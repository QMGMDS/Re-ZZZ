using System;
using System.Collections.Generic;

namespace GamePlay.Team
{
    /// <summary>
    /// 队伍切换请求接受后的只读快照
    /// </summary>
    public sealed class TeamReadOnlyInfo
    {
        private readonly IReadOnlyList<TeamCharacterReadOnlyInfo> _characters;

        /// <summary>
        /// 按队伍顺序排列的角色只读列表
        /// </summary>
        public IReadOnlyList<TeamCharacterReadOnlyInfo> Characters => _characters;

        /// <summary>
        /// 当前角色在队伍中的序号
        /// </summary>
        public int CurrentCharacterIndex { get; }

        /// <summary>
        /// 队伍角色数量
        /// </summary>
        public int CharacterCount => _characters.Count;

        /// <summary>
        /// 构造队伍切换请求接受后的只读快照
        /// </summary>
        internal TeamReadOnlyInfo(
            IReadOnlyList<TeamCharacterReadOnlyInfo> characters,
            int currentCharacterIndex)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            if (characters.Count == 0)
            {
                throw new ArgumentException(
                    "队伍至少需要一个角色",
                    nameof(characters));
            }

            if (currentCharacterIndex < 0
                || currentCharacterIndex >= characters.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentCharacterIndex));
            }

            TeamCharacterReadOnlyInfo[] snapshot =
                new TeamCharacterReadOnlyInfo[characters.Count];

            for (int index = 0; index < characters.Count; index++)
            {
                snapshot[index] = characters[index];
            }

            _characters = Array.AsReadOnly(snapshot);
            CurrentCharacterIndex = currentCharacterIndex;
        }
    }
}
