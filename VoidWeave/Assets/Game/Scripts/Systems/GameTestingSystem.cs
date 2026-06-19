namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GameplaySystemGroup))]
    public partial struct GameTestingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<CurrentEnergyWhileTestingComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<EnemiesToKillIncrementComponent>();
            systemState.RequireForUpdate<EnemiesToKillWhileTestingComponent>();
            systemState.RequireForUpdate<FloatToleranceComponent>();
            systemState.RequireForUpdate<IsTestingComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LevelWhileTestingComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            systemState.RequireForUpdate<TimerWhileTestingComponent>();
            systemState.RequireForUpdate<WaveStatePrepComponent>();
            systemState.RequireForUpdate<WaveStockComponent>();

            systemState.RequireForUpdate<EnemySpawnerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int currentEnergyWhileTesting = SystemAPI.GetSingleton<CurrentEnergyWhileTestingComponent>().Value;
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            int enemiesToKillIncrement = SystemAPI.GetSingleton<EnemiesToKillIncrementComponent>().Value;
            int enemiesToKillWhileTesting = SystemAPI.GetSingleton<EnemiesToKillWhileTestingComponent>().Value;
            float floatTolerance = SystemAPI.GetSingleton<FloatToleranceComponent>().Value;
            int isTesting = SystemAPI.GetSingleton<IsTestingComponent>().Value;
            bool isTestingBool = isTesting == 1;
            int levelWhileTesting = SystemAPI.GetSingleton<LevelWhileTestingComponent>().Value;
            float timerWhileTesting = SystemAPI.GetSingleton<TimerWhileTestingComponent>().Value;
            int waveStatePrep = SystemAPI.GetSingleton<WaveStatePrepComponent>().Value;
            bool isTestingMode = isTestingBool;
            int initialTestState = waveStatePrep;

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<IsTestingTag>>().WithEntityAccess())
            {
                SystemAPI.GetSingletonRW<CurrentEnergyComponent>().ValueRW.Value = math.select(SystemAPI.GetSingleton<CurrentEnergyComponent>().Value , currentEnergyWhileTesting , isTestingBool);
                SystemAPI.GetSingletonRW<LevelComponent>().ValueRW.Value = math.select(SystemAPI.GetSingleton<LevelComponent>().Value , levelWhileTesting , isTestingBool);

                int levelDifference = math.max(0 , levelWhileTesting - 1);
                int fastForwardedEnemiesToKill = SystemAPI.GetSingleton<EnemiesToKillComponent>().Value + levelDifference * enemiesToKillIncrement;
                int assignedEnemiesToKill = math.select(fastForwardedEnemiesToKill , enemiesToKillWhileTesting , isTestingMode);
                SystemAPI.GetSingletonRW<EnemiesToKillComponent>().ValueRW.Value = math.select(SystemAPI.GetSingleton<EnemiesToKillComponent>().Value , assignedEnemiesToKill , isTestingBool);

                foreach(var (timerComponent , waveIndexComponent , waveStateComponent , spawnerEntity) in SystemAPI.Query<RefRW<TimerComponent> , RefRW<WaveIndexComponent> , RefRW<WaveStateComponent>>().WithEntityAccess().WithAll<EnemySpawnerTag>())
                {
                    timerComponent.ValueRW.Value = math.select(timerComponent.ValueRO.Value , timerWhileTesting , isTestingBool);
                    waveIndexComponent.ValueRW.Value = math.select(waveIndexComponent.ValueRO.Value , 0 , isTestingBool);

                    waveStateComponent.ValueRW.Value = math.select(waveStateComponent.ValueRO.Value , initialTestState , isTestingBool);

                    bool hasTutorialTag = SystemAPI.HasComponent<TurretsTutorialActiveTag>(spawnerEntity);
                    int disableTutorial = math.select(0 , 1 , hasTutorialTag & isTestingBool);

                    for(int i = 0 ; i < disableTutorial ; i++) { ecb.SetComponentEnabled<TurretsTutorialActiveTag>(spawnerEntity , false); }
                }

                ecb.SetComponentEnabled<IsTestingTag>(entity , false);
            }

            foreach(var timerComponent in SystemAPI.Query<RefRW<TimerComponent>>().WithAll<EnemySpawnerTag>())
            {
                bool isSystemResettingTimer = math.abs(timerComponent.ValueRO.Value - timerWhileTesting) > floatTolerance && timerComponent.ValueRO.Value > timerWhileTesting;
                timerComponent.ValueRW.Value = math.select(timerComponent.ValueRO.Value , timerWhileTesting , isTestingBool & isSystemResettingTimer);
            }

            foreach(var enemiesToKillComponent in SystemAPI.Query<RefRW<EnemiesToKillComponent>>()) { enemiesToKillComponent.ValueRW.Value = math.select(enemiesToKillComponent.ValueRO.Value , enemiesToKillWhileTesting , isTestingBool); }
        }
    }
}