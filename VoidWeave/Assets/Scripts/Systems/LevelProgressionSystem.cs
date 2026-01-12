namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WaveStateSystem))]
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
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlayerTag>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<UnlockedEnemiesComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<WaveIndexComponent>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency.Complete();

            var enemiesKilledComponent = SystemAPI.GetSingletonRW<EnemiesKilledComponent>();
            var enemiesToKillComponent = SystemAPI.GetSingletonRW<EnemiesToKillComponent>();
            var enemiesToKillIncrementComponent = SystemAPI.GetSingleton<EnemiesToKillIncrementComponent>();
            var levelComponent = SystemAPI.GetSingletonRW<LevelComponent>();
            var unlockedEnemiesComponent = SystemAPI.GetSingletonRW<UnlockedEnemiesComponent>();
            var waveIndexComponent = SystemAPI.GetSingletonRW<WaveIndexComponent>();

            // Check if Kill Count meets the Level Threshold
            bool isLevelComplete = enemiesKilledComponent.ValueRO.KillsCount >= enemiesToKillComponent.ValueRW.EnemiesToKill;

            // Reset Kill Count for the new level
            enemiesKilledComponent.ValueRW.KillsCount = math.select(enemiesKilledComponent.ValueRO.KillsCount , 0 , isLevelComplete);
            enemiesToKillComponent.ValueRW.EnemiesToKill += math.select(0 , enemiesToKillIncrementComponent.EnemiesToKillIncrement , isLevelComplete);

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var currentHealthComponent = SystemAPI.GetComponentRW<CurrentHealthComponent>(playerEntity);
            var maxHealthComponent = SystemAPI.GetComponentRW<MaxHealthComponent>(playerEntity);
            
            currentHealthComponent.ValueRW.CurrentHealth = math.select(currentHealthComponent.ValueRO.CurrentHealth , maxHealthComponent.ValueRO.MaxHealth , isLevelComplete);

            // Increment Level
            levelComponent.ValueRW.Level += math.select(0 , 1 , isLevelComplete);

            //Update Unlocks (Line at Level 2, Square at Level 3)
            uint bitMask = 1;

            bitMask |= (uint)math.select(0 , 2 , levelComponent.ValueRO.Level >= 2);
            bitMask |= (uint)math.select(0 , 4 , levelComponent.ValueRO.Level >= 3);

            unlockedEnemiesComponent.ValueRW.UnlockedEnemiesBitmask = math.select(unlockedEnemiesComponent.ValueRO.UnlockedEnemiesBitmask , bitMask , isLevelComplete);
            waveIndexComponent.ValueRW.Index = math.select(waveIndexComponent.ValueRO.Index , 0 , isLevelComplete);
        }
    }
}