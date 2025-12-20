using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    public partial struct WaveSystem : ISystem
    {
        private EntityQuery _enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag , TeamComponent>().Build();
            
            state.RequireForUpdate<WavePrepDurationComponent>();
            state.RequireForUpdate<WaveStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new WaveLogicJob { DeltaTime = SystemAPI.Time.DeltaTime , AliveEnemyCount = _enemyQuery.CalculateEntityCount() }.ScheduleParallel(); }
    }

    [BurstCompile]
    public partial struct WaveLogicJob : IJobEntity
    {
        public int AliveEnemyCount;
        public float DeltaTime;
        
        private void Execute(ref WaveIndexComponent waveIndexComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent , ref WaveTimerComponent waveTimerComponent , in WavePrepDurationComponent wavePrepDurationComponent , in WaveBaseEnemyCountComponent waveBaseEnemyCountComponent , in WaveEnemyIncrementComponent waveEnemyIncrementComponent)
        {
            waveTimerComponent.Timer -= DeltaTime;
            waveIndexComponent.Index += (int)math.select(0 , 1 , (waveStateComponent.State == 0) && (waveTimerComponent.Timer <= 0f));
            waveStateComponent.State = math.select(math.select(waveStateComponent.State , 1 , (waveStateComponent.State == 0) && (waveTimerComponent.Timer <= 0f)) , 0 , (waveStateComponent.State == 1) && (waveStockComponent.Stock <= 0) && (AliveEnemyCount <= 0));
            waveStockComponent.Stock = math.select(math.select(waveStockComponent.Stock , waveBaseEnemyCountComponent.Count + (waveIndexComponent.Index * waveEnemyIncrementComponent.Count) , (waveStateComponent.State == 0) && (waveTimerComponent.Timer <= 0f)) , 0 , (waveStateComponent.State == 1) && (waveStockComponent.Stock <= 0) && (AliveEnemyCount <= 0));
            waveTimerComponent.Timer = math.select(waveTimerComponent.Timer , wavePrepDurationComponent.Duration , (waveStateComponent.State == 1) && (waveStockComponent.Stock <= 0) && (AliveEnemyCount <= 0));
        }
    }
}