using UnityEngine;

namespace GamePlay.Character.Public
{
    public interface IPlayerCharacterService : ICharacterService
    {
        void EnterField(Transform characterTransform);

        void ExitField();
    }
}
