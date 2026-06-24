namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct WaveStateSystem : ISystem
    {
        private EntityQuery _enemyQuery;
        private EntityQuery _tutorialActiveQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag , TeamComponent>().Build();
            _tutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag , TurretsTutorialActiveTag>().Build();

            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<TimerComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            systemState.RequireForUpdate<Wave1MultiplierComponent>();
            systemState.RequireForUpdate<Wave2MultiplierComponent>();
            systemState.RequireForUpdate<Wave3MultiplierComponent>();
            systemState.RequireForUpdate<WaveBaseEnemyCountComponent>();
            systemState.RequireForUpdate<WaveEnemyIncrementComponent>();
            systemState.RequireForUpdate<WaveIndexComponent>();
            systemState.RequireForUpdate<WavePrepDurationComponent>();
            systemState.RequireForUpdate<WavesPerLevelComponent>();
            systemState.RequireForUpdate<WaveStateCombatComponent>();
            systemState.RequireForUpdate<WaveStateComponent>();
            systemState.RequireForUpdate<WaveStatePrepComponent>();
            systemState.RequireForUpdate<WaveStockComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency = new WaveStateJob
            {
                AliveEnemyCount = _enemyQuery.CalculateEntityCount() ,
                EnemiesKilled = SystemAPI.GetSingleton<EnemiesKilledComponent>().Value ,
                EnemiesToKill = SystemAPI.GetSingleton<EnemiesToKillComponent>().Value ,
                IsTutorialActive = !_tutorialActiveQuery.IsEmpty ,
                TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value ,
                Wave1Multiplier = SystemAPI.GetSingleton<Wave1MultiplierComponent>().Value ,
                Wave2Multiplier = SystemAPI.GetSingleton<Wave2MultiplierComponent>().Value ,
                Wave3Multiplier = SystemAPI.GetSingleton<Wave3MultiplierComponent>().Value ,
                WavesPerLevel = SystemAPI.GetSingleton<WavesPerLevelComponent>().Value ,
                WaveStateCombat = SystemAPI.GetSingleton<WaveStateCombatComponent>().Value ,
                WaveStatePrep = SystemAPI.GetSingleton<WaveStatePrepComponent>().Value
            }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct WaveStateJob : IJobEntity
    {
        public int AliveEnemyCount;
        public int EnemiesKilled;
        public int EnemiesToKill;
        public bool IsTutorialActive;
        public float TimerExpired;
        public float Wave1Multiplier;
        public float Wave2Multiplier;
        public float Wave3Multiplier;
        public int WavesPerLevel;
        public int WaveStateCombat;
        public int WaveStatePrep;

        private void Execute(ref TimerComponent timerComponent , ref WaveIndexComponent waveIndexComponent , in WavePrepDurationComponent wavePrepDurationComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool isLevelOngoing = EnemiesKilled < EnemiesToKill;
            bool isPrepComplete = waveStateComponent.Value == WaveStatePrep && timerComponent.Value <= TimerExpired && !IsTutorialActive;
            bool isWaveClear = waveStateComponent.Value == WaveStateCombat && waveStockComponent.Value <= 0 && AliveEnemyCount <= 0 && isLevelOngoing;

            waveIndexComponent.Value += math.select(0 , 1 , isPrepComplete);
            waveStateComponent.Value = math.select(waveStateComponent.Value , WaveStateCombat , isPrepComplete);
            waveStateComponent.Value = math.select(waveStateComponent.Value , WaveStatePrep , isWaveClear);

            int enemiesNeededForLevel = EnemiesToKill - EnemiesKilled - AliveEnemyCount;

            int safeWavesPerLevel = math.max(1 , WavesPerLevel);
            int currentWaveInLevel = (waveIndexComponent.Value - 1) % safeWavesPerLevel;
            currentWaveInLevel = math.select(currentWaveInLevel , 0 , currentWaveInLevel < 0);

            float multiplier = math.select(math.select(Wave3Multiplier , Wave2Multiplier , currentWaveInLevel == 1) , Wave1Multiplier , currentWaveInLevel == 0);

            int calculatedStock = (int)(enemiesNeededForLevel * multiplier);

            int minRequiredStock = math.select(0 , 1 , enemiesNeededForLevel > 0);

            int cappedStock = math.clamp(calculatedStock , minRequiredStock , enemiesNeededForLevel);

            waveStockComponent.Value = math.select(waveStockComponent.Value , cappedStock , isPrepComplete);
            timerComponent.Value = math.select(timerComponent.Value , wavePrepDurationComponent.Value , isWaveClear);
        }
    }
}