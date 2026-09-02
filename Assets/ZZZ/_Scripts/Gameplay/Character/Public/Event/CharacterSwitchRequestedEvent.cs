using System;

using UnityEngine;

using SPFramework;

namespace GamePlay.Character.Public
{
    public readonly struct CharacterSwitchRequestedEvent : IEvent
    {
        public Transform CharacterTransform { get; }

        public CharacterSwitchRequestedEvent(Transform characterTransform)
        {
            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            CharacterTransform = characterTransform;
        }
    }
}
