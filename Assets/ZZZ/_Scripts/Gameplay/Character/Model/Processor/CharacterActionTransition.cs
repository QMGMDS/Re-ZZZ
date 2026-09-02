using UnityEngine;

namespace GamePlay.Character
{
    public sealed class CharacterActionTransition
    {
        public void Advance(ref CharacterActionState state)
        {
            state.SetLogicalProgressSeconds(state.LogicalProgressSeconds + Time.deltaTime);
        }
    }
}
