namespace Core
{
    public sealed class RoomLifecycle
    {
        public bool RewardSpawned { get; private set; }

        public void Reset()
        {
            RewardSpawned = false;
        }

        public void MarkRewardSpawned()
        {
            RewardSpawned = true;
        }
    }

    public sealed class RoomCombatState
    {
        public bool IsActive { get; private set; }
        public int ActiveEnemyCount { get; private set; }

        public void Reset()
        {
            IsActive = false;
            ActiveEnemyCount = 0;
        }

        public void Activate(int enemyCount)
        {
            IsActive = enemyCount > 0;
            ActiveEnemyCount = enemyCount;
        }

        public void NotifyEnemyDefeated()
        {
            if (ActiveEnemyCount > 0)
                ActiveEnemyCount--;
        }
    }
}