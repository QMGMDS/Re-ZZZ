using UnityEngine;

namespace GamePlay.Character.Public
{
    public interface IPlayerCharacterService : ICharacterService
    {
        void EnterField(int characterEntityId, Transform characterTransform);

        void ExitField();
    }
}
