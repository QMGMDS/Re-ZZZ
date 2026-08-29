using System;

using GamePlay.Character;
using GamePlay.Character.Contract;
using GamePlay.Combat.Contract;

namespace GamePlay.Combat
{
    /// <summary>
    /// 统一处理战斗命中判定
    /// </summary>
    public sealed class CombatModule : ICombatModule
    {
        private readonly ICharacterModule _characterModule;

        public CombatModule(ICharacterModule characterModule)
        {
            if (characterModule == null)
            {
                throw new ArgumentNullException(nameof(characterModule));
            }

            _characterModule = characterModule;
        }

        /// <inheritdoc/>
        public void SubmitHit(int attackerEntityId, int targetEntityId)
        {
            if (attackerEntityId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attackerEntityId));
            }

            if (targetEntityId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetEntityId));
            }

            if (!_characterModule.TryGetCharacterInfoRuntime(
                    attackerEntityId,
                    out CharacterInfoRuntime attackerInfo)
                || !_characterModule.TryGetCharacterInfoRuntime(
                    targetEntityId,
                    out CharacterInfoRuntime targetInfo))
            {
                return;
            }

            if (attackerInfo.Faction == targetInfo.Faction)
            {
                return;
            }

            if (!_characterModule.TryGetCharacterHurtReceiver(
                    targetEntityId,
                    out ICharacterHurtReceiver hurtReceiver))
            {
                return;
            }

            hurtReceiver.ReceiveHit(attackerInfo.BaseAtk);
        }
    }
}
