namespace Game.Scripts.Systems
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
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
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
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int enemiesKilled = SystemAPI.GetSingleton<EnemiesKilledComponent>().Value;
            int enemiesToKill = SystemAPI.GetSingleton<EnemiesToKillComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;
            int waveStateCombat = SystemAPI.GetSingleton<WaveStateCombatComponent>().Value;
            int waveStatePrep = SystemAPI.GetSingleton<WaveStatePrepComponent>().Value;

            systemState.Dependency = new WaveStateJob { AliveEnemyCount = _enemyQuery.CalculateEntityCount() , DoAction = doAction , EnemiesKilled = enemiesKilled , EnemiesToKill = enemiesToKill , NoAction = noAction , TimerExpired = timerExpired , WaveStateCombat = waveStateCombat , WaveStatePrep = waveStatePrep }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct WaveStateJob : IJobEntity
    {
        public int AliveEnemyCount;
        public int DoAction;
        public int EnemiesKilled;
        public int EnemiesToKill;
        public int NoAction;
        public float TimerExpired;
        public int WaveStateCombat;
        public int WaveStatePrep;

        private void Execute(ref TimerComponent timerComponent , in WaveBaseEnemyCountComponent waveBaseEnemyCountComponent , in WaveEnemyIncrementComponent waveEnemyIncrementComponent , ref WaveIndexComponent waveIndexComponent , in WavePrepDurationComponent wavePrepDurationComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool isPrepComplete = waveStateComponent.Value == WaveStatePrep && timerComponent.Value <= TimerExpired;
            bool isWaveClear = waveStateComponent.Value == WaveStateCombat && waveStockComponent.Value <= NoAction && AliveEnemyCount <= NoAction;

            waveIndexComponent.Value += math.select(NoAction , DoAction , isPrepComplete);
            waveStateComponent.Value = math.select(waveStateComponent.Value , WaveStateCombat , isPrepComplete);
            waveStateComponent.Value = math.select(waveStateComponent.Value , WaveStatePrep , isWaveClear);

            int enemiesNeededForLevel = EnemiesToKill - EnemiesKilled - AliveEnemyCount;
            int calculatedStock = waveBaseEnemyCountComponent.Value + waveIndexComponent.Value * waveEnemyIncrementComponent.Value;
            int cappedStock = math.max(NoAction , math.min(calculatedStock , enemiesNeededForLevel));
            
            waveStockComponent.Value = math.select(waveStockComponent.Value , cappedStock , isPrepComplete);
            timerComponent.Value = math.select(timerComponent.Value , wavePrepDurationComponent.Value , isWaveClear);
        }
    }
}