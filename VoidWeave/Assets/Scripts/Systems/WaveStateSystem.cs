namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    [UpdateBefore(typeof(WaveTimerSystem))]
    public partial struct WaveStateSystem : ISystem
    {
        private EntityQuery _enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag , TeamComponent>().Build();

            systemState.RequireForUpdate<WaveBaseEnemyCountComponent>();
            systemState.RequireForUpdate<WaveEnemyIncrementComponent>();
            systemState.RequireForUpdate<WaveIndexComponent>();
            systemState.RequireForUpdate<WavePrepDurationComponent>();
            systemState.RequireForUpdate<WaveStateComponent>();
            systemState.RequireForUpdate<WaveStockComponent>();
            systemState.RequireForUpdate<WaveTimerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new WaveStateJob { AliveEnemyCount = _enemyQuery.CalculateEntityCount() }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct WaveStateJob : IJobEntity
    {
        public int AliveEnemyCount;

        private void Execute(in WaveBaseEnemyCountComponent waveBaseEnemyCountComponent , in WaveEnemyIncrementComponent waveEnemyIncrementComponent , ref WaveIndexComponent waveIndexComponent , in WavePrepDurationComponent wavePrepDurationComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent , ref WaveTimerComponent waveTimerComponent)
        {
            bool isPrepComplete = (waveStateComponent.State == 0) && (waveTimerComponent.Timer <= 0f);
            bool isWaveClear = (waveStateComponent.State == 1) && (waveStockComponent.Stock <= 0) && (AliveEnemyCount <= 0);
            
            waveStateComponent.State = math.select(waveStateComponent.State , 1 , isPrepComplete);
            waveStateComponent.State = math.select(waveStateComponent.State , 0 , isWaveClear);
            waveIndexComponent.Index += math.select(0 , 1 , isPrepComplete);
            
            int newStock = waveBaseEnemyCountComponent.Count + (waveIndexComponent.Index * waveEnemyIncrementComponent.Count);
            waveStockComponent.Stock = math.select(waveStockComponent.Stock , newStock , isPrepComplete);
            waveTimerComponent.Timer = math.select(waveTimerComponent.Timer , wavePrepDurationComponent.Duration , isWaveClear);
        }
    }
}