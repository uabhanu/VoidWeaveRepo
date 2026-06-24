namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(WaveStateSystem))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    public partial struct AdvanceLevelSystem : ISystem
    {
        private EntityQuery _advanceLevelQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _advanceLevelQuery = SystemAPI.QueryBuilder().WithAll<AdvanceLevelEventTag>().Build();
            
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<EnemiesToKillIncrementComponent>();
            systemState.RequireForUpdate<IsTestingComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LevelToUnlockLineEnemyComponent>();
            systemState.RequireForUpdate<LastLevelComponent>();
            systemState.RequireForUpdate<PlayerTag>();
            systemState.RequireForUpdate<LevelToUnlockSquareEnemyComponent>();
            systemState.RequireForUpdate<LevelToUnlockTriangleEnemyComponent>();
            systemState.RequireForUpdate<UnlockedEnemiesComponent>();
            systemState.RequireForUpdate<UnlockedLineEnemyComponent>();
            systemState.RequireForUpdate<UnlockedSquareEnemyComponent>();
            systemState.RequireForUpdate<UnlockedTriangleEnemyComponent>();
            systemState.RequireForUpdate<WaveIndexComponent>();

            systemState.RequireForUpdate<AdvanceLevelEventTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency.Complete();
            
            int isTesting = SystemAPI.GetSingleton<IsTestingComponent>().Value;
            bool isTestingBool = isTesting == 1;
            int levelToUnlockLineEnemy = SystemAPI.GetSingleton<LevelToUnlockLineEnemyComponent>().Value;
            int levelToUnlockSquareEnemy = SystemAPI.GetSingleton<LevelToUnlockSquareEnemyComponent>().Value;
            int levelToUnlockTriangleEnemy = SystemAPI.GetSingleton<LevelToUnlockTriangleEnemyComponent>().Value;
            int lastLevel = SystemAPI.GetSingleton<LastLevelComponent>().Value;
            uint unlockedLineEnemy = SystemAPI.GetSingleton<UnlockedLineEnemyComponent>().Value;
            uint unlockedSquareEnemy = SystemAPI.GetSingleton<UnlockedSquareEnemyComponent>().Value;
            uint unlockedTriangleEnemy = SystemAPI.GetSingleton<UnlockedTriangleEnemyComponent>().Value;

            RefRW<EnemiesKilledComponent> enemiesKilledComponent = SystemAPI.GetSingletonRW<EnemiesKilledComponent>();
            RefRW<EnemiesToKillComponent> enemiesToKillComponent = SystemAPI.GetSingletonRW<EnemiesToKillComponent>();
            var enemiesToKillIncrementComponent = SystemAPI.GetSingleton<EnemiesToKillIncrementComponent>();
            RefRW<LevelComponent> levelComponent = SystemAPI.GetSingletonRW<LevelComponent>();
            RefRW<UnlockedEnemiesComponent> unlockedEnemiesComponent = SystemAPI.GetSingletonRW<UnlockedEnemiesComponent>();
            RefRW<WaveIndexComponent> waveIndexComponent = SystemAPI.GetSingletonRW<WaveIndexComponent>();

            bool isLevelComplete = enemiesKilledComponent.ValueRO.Value >= enemiesToKillComponent.ValueRO.Value;

            enemiesKilledComponent.ValueRW.Value = math.select(enemiesKilledComponent.ValueRO.Value , 0 , isLevelComplete);
            enemiesToKillComponent.ValueRW.Value += math.select(0 , enemiesToKillIncrementComponent.Value , isLevelComplete);

            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();

            RefRW<SelectedTurretEntityComponent> selectedTurretEntityComponent = SystemAPI.GetComponentRW<SelectedTurretEntityComponent>(playerEntity);
            RefRW<SelectedTurretCostComponent> selectedTurretCostComponent = SystemAPI.GetComponentRW<SelectedTurretCostComponent>(playerEntity);

            int turretEntityIndex = math.select(selectedTurretEntityComponent.ValueRO.Entity.Index , -1 , isLevelComplete);
            int turretEntityVersion = math.select(selectedTurretEntityComponent.ValueRO.Entity.Version , 0 , isLevelComplete);

            selectedTurretEntityComponent.ValueRW.Entity = new Entity { Index = turretEntityIndex , Version = turretEntityVersion };
            selectedTurretCostComponent.ValueRW.Value = math.select(selectedTurretCostComponent.ValueRO.Value , 0 , isLevelComplete);

            bool isNotLastLevel = levelComponent.ValueRO.Value < lastLevel;
            levelComponent.ValueRW.Value += math.select(0 , 1 , isLevelComplete & isNotLastLevel);
            
            uint bitMask = 0;
            bitMask |= math.select(0 , unlockedLineEnemy , levelComponent.ValueRO.Value >= levelToUnlockLineEnemy);
            bitMask |= math.select(0 , unlockedTriangleEnemy , levelComponent.ValueRO.Value >= levelToUnlockTriangleEnemy);
            bitMask |= math.select(0 , unlockedSquareEnemy , levelComponent.ValueRO.Value >= levelToUnlockSquareEnemy);

            bool shouldUpdateMask = isLevelComplete || isTestingBool;
            unlockedEnemiesComponent.ValueRW.Value = math.select(unlockedEnemiesComponent.ValueRO.Value , bitMask , shouldUpdateMask);
            waveIndexComponent.ValueRW.Value = math.select(waveIndexComponent.ValueRO.Value , 0 , isLevelComplete);

            systemState.EntityManager.DestroyEntity(_advanceLevelQuery);
        }
    }
}