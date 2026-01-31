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
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<EnemiesToKillIncrementComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LevelToUnlockLineEnemyComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<PlayerTag>();
            systemState.RequireForUpdate<LevelToUnlockSquareEnemyComponent>();
            systemState.RequireForUpdate<LevelToUnlockTriangleEnemyComponent>();
            systemState.RequireForUpdate<UnlockedEnemiesComponent>();
            systemState.RequireForUpdate<UnlockedLineEnemyComponent>();
            systemState.RequireForUpdate<UnlockedNoneComponent>();
            systemState.RequireForUpdate<UnlockedSquareEnemyComponent>();
            systemState.RequireForUpdate<UnlockedTriangleEnemyComponent>();
            systemState.RequireForUpdate<WaveIndexComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency.Complete();
            
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().DoAction;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().NoActionValue;
            int levelToUnlockLineEnemy = SystemAPI.GetSingleton<LevelToUnlockLineEnemyComponent>().LevelToUnlockLineEnemy;
            int levelToUnlockSquareEnemy = SystemAPI.GetSingleton<LevelToUnlockSquareEnemyComponent>().LevelToUnlockSquareEnemy;
            int levelToUnlockTriangleEnemy = SystemAPI.GetSingleton<LevelToUnlockTriangleEnemyComponent>().LevelToUnlockTriangleEnemy;
            uint unlockedLineEnemy = SystemAPI.GetSingleton<UnlockedLineEnemyComponent>().UnlockedLineEnemy;
            uint unlockedNone = SystemAPI.GetSingleton<UnlockedNoneComponent>().UnlockedNone;
            uint unlockedSquareEnemy = SystemAPI.GetSingleton<UnlockedSquareEnemyComponent>().UnlockedSquareEnemy;
            uint unlockedTriangleEnemy = SystemAPI.GetSingleton<UnlockedTriangleEnemyComponent>().UnlockedTriangleEnemy;

            var enemiesKilledComponent = SystemAPI.GetSingletonRW<EnemiesKilledComponent>();
            var enemiesToKillComponent = SystemAPI.GetSingletonRW<EnemiesToKillComponent>();
            var enemiesToKillIncrementComponent = SystemAPI.GetSingleton<EnemiesToKillIncrementComponent>();
            var levelComponent = SystemAPI.GetSingletonRW<LevelComponent>();
            var unlockedEnemiesComponent = SystemAPI.GetSingletonRW<UnlockedEnemiesComponent>();
            var waveIndexComponent = SystemAPI.GetSingletonRW<WaveIndexComponent>();

            // Check if Kill Count meets the Level Threshold
            bool isLevelComplete = enemiesKilledComponent.ValueRO.KillsCount >= enemiesToKillComponent.ValueRW.EnemiesToKill;

            // Reset Kill Count for the new level
            enemiesKilledComponent.ValueRW.KillsCount = math.select(enemiesKilledComponent.ValueRO.KillsCount , noAction , isLevelComplete);
            enemiesToKillComponent.ValueRW.EnemiesToKill += math.select(noAction , enemiesToKillIncrementComponent.EnemiesToKillIncrement , isLevelComplete);

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var currentHealthComponent = SystemAPI.GetComponentRW<CurrentHealthComponent>(playerEntity);
            var maxHealthComponent = SystemAPI.GetComponentRW<MaxHealthComponent>(playerEntity);
            
            currentHealthComponent.ValueRW.CurrentHealth = math.select(currentHealthComponent.ValueRO.CurrentHealth , maxHealthComponent.ValueRO.MaxHealth , isLevelComplete);

            // Increment Level
            levelComponent.ValueRW.Level += math.select(noAction , doAction , isLevelComplete);

            //Update Unlocks (Triangle Enemy unlocks at Level 2, Square Enemy at Level 3)
            uint bitMask = unlockedNone;
            bitMask |= (uint)math.select(noAction , unlockedLineEnemy , levelComponent.ValueRO.Level >= levelToUnlockLineEnemy);
            bitMask |= (uint)math.select(noAction , unlockedTriangleEnemy , levelComponent.ValueRO.Level >= levelToUnlockTriangleEnemy);
            bitMask |= (uint)math.select(noAction , unlockedSquareEnemy , levelComponent.ValueRO.Level >= levelToUnlockSquareEnemy);

            unlockedEnemiesComponent.ValueRW.UnlockedEnemiesBitmask = math.select(unlockedEnemiesComponent.ValueRO.UnlockedEnemiesBitmask , bitMask , isLevelComplete);
            waveIndexComponent.ValueRW.Index = math.select(waveIndexComponent.ValueRO.Index , noAction , isLevelComplete);
        }
    }
}