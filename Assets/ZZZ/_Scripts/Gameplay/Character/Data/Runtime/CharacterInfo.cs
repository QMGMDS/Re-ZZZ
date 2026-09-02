namespace GamePlay.Character
{
    public sealed class CharacterInfo
    {
        public string CharacterConfigId { get; }

        public int EntityId { get; }

        public int BaseHealth { get; }

        public int CurrentHealth { get; private set; }

        public int BaseAttack { get; }

        public int CurrentAttack { get; private set; }

        public CharacterInfo(
            string configId,
            int entityId,
            int baseHealth,
            int currentHealth,
            int baseAttack,
            int currentAttack)
        {
            if (string.IsNullOrWhiteSpace(configId))
            {
                throw new System.ArgumentException("角色配置 ID 不能为空", nameof(configId));
            }

            if (entityId < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(entityId));
            }

            if (baseHealth <= 0 || currentHealth < 0 || currentHealth > baseHealth)
            {
                throw new System.ArgumentOutOfRangeException(nameof(currentHealth));
            }

            if (baseAttack < 0 || currentAttack < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(currentAttack));
            }

            CharacterConfigId = configId;
            EntityId = entityId;
            BaseHealth = baseHealth;
            CurrentHealth = currentHealth;
            BaseAttack = baseAttack;
            CurrentAttack = currentAttack;
        }

        public void SetCurrentHealth(int currentHealth)
        {
            if (currentHealth < 0 || currentHealth > BaseHealth)
            {
                throw new System.ArgumentOutOfRangeException(nameof(currentHealth));
            }

            CurrentHealth = currentHealth;
        }

        public void SetCurrentAttack(int currentAttack)
        {
            if (currentAttack < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(currentAttack));
            }

            CurrentAttack = currentAttack;
        }
    }
}
