namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(ManagedEventBridgeSystem))]
    public partial struct GameTestingSystem : ISystem
    {
        private EntityQuery _enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build();

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<CurrentEnergyWhileTestingComponent>();
            systemState.RequireForUpdate<DoActionComponent>();
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
            int currentEnergyWhileTesting = SystemAPI.GetSingleton<CurrentEnergyWhileTestingComponent>().Value;
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            int enemiesInTheSceneCount = _enemyQuery.CalculateEntityCount();
            int isTesting = SystemAPI.GetSingleton<IsTestingComponent>().Value;
            bool isTestingBool = isTesting == doAction;
            int levelWhileTesting = SystemAPI.GetSingleton<LevelWhileTestingComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerWhileTesting = SystemAPI.GetSingleton<TimerWhileTestingComponent>().Value;
            int waveStateWhileTesting = SystemAPI.GetSingleton<WaveStateWhileTestingComponent>().Value;
            int waveStockWhileTesting = SystemAPI.GetSingleton<WaveStockWhileTestingComponent>().Value;

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<IsTestingTag>>().WithEntityAccess())
            {
                SystemAPI.GetSingletonRW<CurrentEnergyComponent>().ValueRW.Value = math.select(SystemAPI.GetSingleton<CurrentEnergyComponent>().Value , currentEnergyWhileTesting , isTestingBool);
                SystemAPI.GetSingletonRW<LevelComponent>().ValueRW.Value = math.select(SystemAPI.GetSingleton<LevelComponent>().Value , levelWhileTesting , isTestingBool);

                foreach(var (timerComponent , waveIndexComponent , waveStateComponent) in SystemAPI.Query<RefRW<TimerComponent> , RefRW<WaveIndexComponent> , RefRW<WaveStateComponent>>().WithAll<EnemySpawnerTag>())
                {
                    timerComponent.ValueRW.Value = math.select(timerComponent.ValueRO.Value , timerWhileTesting , isTestingBool);
                    waveIndexComponent.ValueRW.Value = math.select(waveIndexComponent.ValueRO.Value , noAction , isTestingBool);
                    waveStateComponent.ValueRW.Value = math.select(waveStateComponent.ValueRO.Value , waveStateWhileTesting , isTestingBool);
                }

                ecb.SetComponentEnabled<IsTestingTag>(entity , false);
            }

            foreach(var waveStockComponent in SystemAPI.Query<RefRW<WaveStockComponent>>().WithAll<EnemySpawnerTag>())
            {
                bool shouldSpawn = isTestingBool & (enemiesInTheSceneCount <= noAction);
                waveStockComponent.ValueRW.Value = math.select(waveStockComponent.ValueRO.Value , waveStockWhileTesting , shouldSpawn);
            }
        }
    }
}