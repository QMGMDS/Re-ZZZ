using System;

namespace GamePlay.Character
{
    public sealed class CharacterInfoCalculator
    {
        public CharacterInfo CalculateInitialInfo(CharacterInfoAsset asset, int entityId)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            asset.Validate();

            return new CharacterInfo(
                asset.CharacterConfigId,
                entityId,
                asset.BaseHealth,
                asset.BaseHealth,
                asset.BaseAttack,
                asset.BaseAttack);
        }
    }
}
