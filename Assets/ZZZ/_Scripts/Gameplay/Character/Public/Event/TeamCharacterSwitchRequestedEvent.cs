using System;

using UnityEngine;

using SPFramework;

namespace GamePlay.Character.Contract
{
    /// <summary>
    /// 角色请求切换事件
    /// </summary>
    public readonly struct TeamCharacterSwitchRequestedEvent : IEvent
    {
        /// <summary>
        /// 发起请求的角色 Transform
        /// </summary>
        public Transform CharacterTransform { get; }

        /// <summary>
        /// 创建角色请求切换事件
        /// </summary>
        /// <param name="characterTransform">发起请求的角色 Transform</param>
        public TeamCharacterSwitchRequestedEvent(Transform characterTransform)
        {
            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            CharacterTransform = characterTransform;
        }
    }
}
