namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WaveSystem))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    public partial struct LevelProgressionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemiesKilledComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemiesToKillComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemiesToKillIncrementComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<LevelComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<UnlockedEnemiesComponent>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var enemiesKilledComponent = SystemAPI.GetSingletonRW<EnemiesKilledComponent>();
            var enemiesToKillComponent = SystemAPI.GetSingletonRW<EnemiesToKillComponent>();
            var enemiesToKillIncrementComponent = SystemAPI.GetSingleton<EnemiesToKillIncrementComponent>();
            var levelComponent = SystemAPI.GetSingletonRW<LevelComponent>();
            var unlockedEnemiesComponent = SystemAPI.GetSingletonRW<UnlockedEnemiesComponent>();

            // Check if Kill Count meets the Level Threshold
            bool isLevelComplete = enemiesKilledComponent.ValueRO.KillsCount >= enemiesToKillComponent.ValueRW.EnemiesToKill;

            // Increment Level
            levelComponent.ValueRW.Level += math.select(0 , 1 , isLevelComplete);

            // Reset Kill Count for the new level
            enemiesKilledComponent.ValueRW.KillsCount = math.select(enemiesKilledComponent.ValueRO.KillsCount , 0 , isLevelComplete);
            
            enemiesToKillComponent.ValueRW.EnemiesToKill += math.select(0 , enemiesToKillIncrementComponent.EnemiesToKillIncrement , isLevelComplete);

            //Update Unlocks (Line at Level 2, Square at Level 3)
            uint bitMask = 1;
            
            bitMask |= (uint)math.select(0 , 2 , levelComponent.ValueRO.Level >= 2);
            bitMask |= (uint)math.select(0 , 4 , levelComponent.ValueRO.Level >= 3);

            unlockedEnemiesComponent.ValueRW.UnlockedEnemiesBitmask = math.select(unlockedEnemiesComponent.ValueRO.UnlockedEnemiesBitmask , bitMask , isLevelComplete);
        }
    }
}