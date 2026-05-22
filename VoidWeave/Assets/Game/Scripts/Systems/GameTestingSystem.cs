namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(LevelProgressionSystem))]
    [UpdateBefore(typeof(WaveStateSystem))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    public partial struct GameTestingSystem : ISystem
    {
        private EntityQuery _enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build();

            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<CurrentEnergyWhileTestingComponent>();
            systemState.RequireForUpdate<IsTestingComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LevelWhileTestingComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            systemState.RequireForUpdate<TimerWhileTestingComponent>();
            systemState.RequireForUpdate<WaveStateCombatComponent>();
            systemState.RequireForUpdate<WaveStateWhileTestingComponent>();
            systemState.RequireForUpdate<WaveStockComponent>();
            systemState.RequireForUpdate<WaveStockWhileTestingComponent>();

            systemState.RequireForUpdate<EnemySpawnerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var isTestingComponent = SystemAPI.GetSingleton<IsTestingComponent>().Value;
            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            int currentEnergyWhileTestingComponent = SystemAPI.GetSingleton<CurrentEnergyWhileTestingComponent>().Value;
            int enemiesInTheSceneCount = _enemyQuery.CalculateEntityCount();
            var levelComponent = SystemAPI.GetSingletonRW<LevelComponent>();
            int levelWhileTestingComponent = SystemAPI.GetSingleton<LevelWhileTestingComponent>().Value;
            int noActionComponent = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerWhileTestingComponent = SystemAPI.GetSingleton<TimerWhileTestingComponent>().Value;
            int waveStateWhileTestingComponent = SystemAPI.GetSingleton<WaveStateWhileTestingComponent>().Value;
            int waveStockWhileTestingComponent = SystemAPI.GetSingleton<WaveStockWhileTestingComponent>().Value;

            currentEnergyComponent.ValueRW.Value = math.select(currentEnergyComponent.ValueRO.Value , currentEnergyWhileTestingComponent , isTestingComponent);
            levelComponent.ValueRW.Value = math.select(levelComponent.ValueRO.Value , levelWhileTestingComponent , isTestingComponent);

            foreach(var (timerComponent , waveIndexComponent , waveStateComponent , waveStockComponent) in SystemAPI.Query<RefRW<TimerComponent> , RefRW<WaveIndexComponent> , RefRW<WaveStateComponent> , RefRW<WaveStockComponent>>().WithAll<EnemySpawnerTag>())
            {
                timerComponent.ValueRW.Value = math.select(timerComponent.ValueRO.Value , timerWhileTestingComponent , isTestingComponent);
                waveIndexComponent.ValueRW.Value = math.select(waveIndexComponent.ValueRO.Value , noActionComponent , isTestingComponent);
                waveStateComponent.ValueRW.Value = math.select(waveStateComponent.ValueRO.Value , waveStateWhileTestingComponent , isTestingComponent);

                bool shouldSpawn = isTestingComponent && enemiesInTheSceneCount <= noActionComponent;
                waveStockComponent.ValueRW.Value = math.select(waveStockComponent.ValueRO.Value , waveStockWhileTestingComponent , shouldSpawn);
            }
        }
    }
}