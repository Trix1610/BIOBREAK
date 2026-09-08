using Core;

namespace Room
{
    public sealed class RoomController
    {
        private readonly RoomCombatState combatState = new();
        private readonly RoomLifecycle lifecycle = new();
        private readonly RoomEnemySpawner enemySpawner;
        private readonly RoomRewardService rewardService;

        public RoomController(
            RoomEnemySpawner enemySpawner,
            RoomRewardService rewardService)
        {
            this.enemySpawner = enemySpawner;
            this.rewardService = rewardService;
        }

        public bool IsActive => combatState.IsActive;
        public bool RewardSpawned => lifecycle.RewardSpawned;

        public void Reset()
        {
            combatState.Reset();
            lifecycle.Reset();
        }

        public bool TryActivate()
        {
            if (combatState.IsActive)
                return true;

            int enemiesSpawned = enemySpawner?.Spawn() ?? 0;
            combatState.Activate(enemiesSpawned);
            return enemiesSpawned > 0;
        }

        public bool AreEnemiesCleared()
        {
            return combatState.ActiveEnemyCount == 0;
        }

        public void NotifyEnemyDefeated()
        {
            combatState.NotifyEnemyDefeated();
        }

        public void MarkRewardSpawned()
        {
            lifecycle.MarkRewardSpawned();
        }

        public void SpawnReward(string roomName)
        {
            rewardService?.SpawnForRoom(roomName);
        }
    }
}