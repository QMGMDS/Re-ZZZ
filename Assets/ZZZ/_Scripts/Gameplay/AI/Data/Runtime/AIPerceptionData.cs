using UnityEngine;

namespace GamePlay.AI
{
    public sealed class AIPerceptionData
    {
        public Vector3 PlayerPositionInWorld { get; private set; }

        public void SetPlayerPositionInWorld(Vector3 playerPositionInWorld)
        {
            PlayerPositionInWorld = playerPositionInWorld;
        }
    }
}
