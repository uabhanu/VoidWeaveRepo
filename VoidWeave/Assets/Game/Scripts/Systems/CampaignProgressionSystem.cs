namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(WaveStateSystem))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    public partial struct CampaignProgressionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<EnemiesToKillIncrementComponent>();
            systemState.RequireForUpdate<IsTestingComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LevelToUnlockLineEnemyComponent>();
            systemState.RequireForUpdate<LastLevelComponent>();
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

            systemState.RequireForUpdate<AdvanceLevelEventTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency.Complete();

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int isTesting = SystemAPI.GetSingleton<IsTestingComponent>().Value;
            bool isTestingBool = isTesting == doAction;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            int levelToUnlockLineEnemy = SystemAPI.GetSingleton<LevelToUnlockLineEnemyComponent>().Value;
            int levelToUnlockSquareEnemy = SystemAPI.GetSingleton<LevelToUnlockSquareEnemyComponent>().Value;
            int levelToUnlockTriangleEnemy = SystemAPI.GetSingleton<LevelToUnlockTriangleEnemyComponent>().Value;
            int maxCampaignLevel = SystemAPI.GetSingleton<LastLevelComponent>().Value;
            uint unlockedLineEnemy = SystemAPI.GetSingleton<UnlockedLineEnemyComponent>().Value;
            uint unlockedNone = SystemAPI.GetSingleton<UnlockedNoneComponent>().Value;
            uint unlockedSquareEnemy = SystemAPI.GetSingleton<UnlockedSquareEnemyComponent>().Value;
            uint unlockedTriangleEnemy = SystemAPI.GetSingleton<UnlockedTriangleEnemyComponent>().Value;

            RefRW<EnemiesKilledComponent> enemiesKilledComponent = SystemAPI.GetSingletonRW<EnemiesKilledComponent>();
            RefRW<EnemiesToKillComponent> enemiesToKillComponent = SystemAPI.GetSingletonRW<EnemiesToKillComponent>();
            var enemiesToKillIncrementComponent = SystemAPI.GetSingleton<EnemiesToKillIncrementComponent>();
            RefRW<LevelComponent> levelComponent = SystemAPI.GetSingletonRW<LevelComponent>();
            RefRW<UnlockedEnemiesComponent> unlockedEnemiesComponent = SystemAPI.GetSingletonRW<UnlockedEnemiesComponent>();
            RefRW<WaveIndexComponent> waveIndexComponent = SystemAPI.GetSingletonRW<WaveIndexComponent>();

            bool isLevelComplete = enemiesKilledComponent.ValueRO.Value >= enemiesToKillComponent.ValueRO.Value;

            enemiesKilledComponent.ValueRW.Value = math.select(enemiesKilledComponent.ValueRO.Value , noAction , isLevelComplete);
            enemiesToKillComponent.ValueRW.Value += math.select(noAction , enemiesToKillIncrementComponent.Value , isLevelComplete);

            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            RefRW<CurrentHealthComponent> currentHealthComponent = SystemAPI.GetComponentRW<CurrentHealthComponent>(playerEntity);
            RefRW<MaxHealthComponent> maxHealthComponent = SystemAPI.GetComponentRW<MaxHealthComponent>(playerEntity);

            RefRW<SelectedTurretEntityComponent> selectedTurretEntityComponent = SystemAPI.GetComponentRW<SelectedTurretEntityComponent>(playerEntity);
            RefRW<SelectedTurretCostComponent> selectedTurretCostComponent = SystemAPI.GetComponentRW<SelectedTurretCostComponent>(playerEntity);

            int turretEntityIndex = math.select(selectedTurretEntityComponent.ValueRO.Entity.Index , -doAction , isLevelComplete);
            int turretEntityVersion = math.select(selectedTurretEntityComponent.ValueRO.Entity.Version , noAction , isLevelComplete);

            selectedTurretEntityComponent.ValueRW.Entity = new Entity { Index = turretEntityIndex , Version = turretEntityVersion };
            selectedTurretCostComponent.ValueRW.Value = math.select(selectedTurretCostComponent.ValueRO.Value , noAction , isLevelComplete);

            currentHealthComponent.ValueRW.Value = math.select(currentHealthComponent.ValueRO.Value , maxHealthComponent.ValueRO.Value , isLevelComplete);

            bool isNotMaxLevel = levelComponent.ValueRO.Value < maxCampaignLevel;
            levelComponent.ValueRW.Value += math.select(noAction , doAction , isLevelComplete & isNotMaxLevel);

            uint bitMask = unlockedNone;
            bitMask |= (uint)math.select(noAction , unlockedLineEnemy , levelComponent.ValueRO.Value >= levelToUnlockLineEnemy);
            bitMask |= (uint)math.select(noAction , unlockedTriangleEnemy , levelComponent.ValueRO.Value >= levelToUnlockTriangleEnemy);
            bitMask |= (uint)math.select(noAction , unlockedSquareEnemy , levelComponent.ValueRO.Value >= levelToUnlockSquareEnemy);

            bool shouldUpdateMask = isLevelComplete || isTestingBool;
            unlockedEnemiesComponent.ValueRW.Value = math.select(unlockedEnemiesComponent.ValueRO.Value , bitMask , shouldUpdateMask);
            waveIndexComponent.ValueRW.Value = math.select(waveIndexComponent.ValueRO.Value , noAction , isLevelComplete);

            var advanceLevelQuery = SystemAPI.QueryBuilder().WithAll<AdvanceLevelEventTag>().Build();
            systemState.EntityManager.DestroyEntity(advanceLevelQuery);
        }
    }
}