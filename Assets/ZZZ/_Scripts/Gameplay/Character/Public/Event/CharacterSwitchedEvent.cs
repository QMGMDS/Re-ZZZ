using System;

using UnityEngine;

using SPFramework;

namespace GamePlay.Character.Public
{
    public readonly struct CharacterSwitchedEvent : IEvent
    {
        public Transform CharacterTransform { get; }

        public CharacterSwitchedEvent(Transform characterTransform)
        {
            if (characterTransform == null)
            {
                throw new ArgumentNullException(nameof(characterTransform));
            }

            CharacterTransform = characterTransform;
        }
    }
}
