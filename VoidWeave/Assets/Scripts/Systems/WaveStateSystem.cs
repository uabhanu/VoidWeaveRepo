namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WaveStateSystem : ISystem
    {
        private EntityQuery _enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag , TeamComponent>().Build();

            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<TimerComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            systemState.RequireForUpdate<WaveBaseEnemyCountComponent>();
            systemState.RequireForUpdate<WaveEnemyIncrementComponent>();
            systemState.RequireForUpdate<WaveIndexComponent>();
            systemState.RequireForUpdate<WavePrepDurationComponent>();
            systemState.RequireForUpdate<WaveStateCombatComponent>();
            systemState.RequireForUpdate<WaveStateComponent>();
            systemState.RequireForUpdate<WaveStatePrepComponent>();
            systemState.RequireForUpdate<WaveStockComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().DoAction;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().NoActionValue;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Expired;
            int waveStateCombat = SystemAPI.GetSingleton<WaveStateCombatComponent>().State;
            int waveStatePrep = SystemAPI.GetSingleton<WaveStatePrepComponent>().State;

            systemState.Dependency = new WaveStateJob { AliveEnemyCount = _enemyQuery.CalculateEntityCount() , DoAction = doAction , NoAction = noAction , TimerExpired = timerExpired , WaveStateCombat = waveStateCombat , WaveStatePrep = waveStatePrep }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct WaveStateJob : IJobEntity
    {
        public int AliveEnemyCount;
        public int DoAction;
        public int NoAction;
        public float TimerExpired;
        public int WaveStateCombat;
        public int WaveStatePrep;

        private void Execute(ref TimerComponent timerComponent , in WaveBaseEnemyCountComponent waveBaseEnemyCountComponent , in WaveEnemyIncrementComponent waveEnemyIncrementComponent , ref WaveIndexComponent waveIndexComponent , in WavePrepDurationComponent wavePrepDurationComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool isPrepComplete = waveStateComponent.State == WaveStatePrep && timerComponent.Timer <= TimerExpired;
            bool isWaveClear = waveStateComponent.State == WaveStateCombat && waveStockComponent.Stock <= NoAction && (AliveEnemyCount <= NoAction);
            
            waveIndexComponent.Index += math.select(NoAction , DoAction , isPrepComplete);
            waveStateComponent.State = math.select(waveStateComponent.State , WaveStateCombat , isPrepComplete);
            waveStateComponent.State = math.select(waveStateComponent.State , WaveStatePrep , isWaveClear);

            int newStock = waveBaseEnemyCountComponent.Count + (waveIndexComponent.Index * waveEnemyIncrementComponent.Count);
            waveStockComponent.Stock = math.select(waveStockComponent.Stock , newStock , isPrepComplete);
            timerComponent.Timer = math.select(timerComponent.Timer , wavePrepDurationComponent.Duration , isWaveClear);
        }
    }
}