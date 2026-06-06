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

            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
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
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int enemiesKilled = SystemAPI.GetSingleton<EnemiesKilledComponent>().Value;
            int enemiesToKill = SystemAPI.GetSingleton<EnemiesToKillComponent>().Value;
            bool isTutorialActive = !_tutorialActiveQuery.IsEmpty;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;
            float wave1Multiplier = SystemAPI.GetSingleton<Wave1MultiplierComponent>().Value;
            float wave2Multiplier = SystemAPI.GetSingleton<Wave2MultiplierComponent>().Value;
            float wave3Multiplier = SystemAPI.GetSingleton<Wave3MultiplierComponent>().Value;
            int wavesPerLevel = SystemAPI.GetSingleton<WavesPerLevelComponent>().Value;
            int waveStateCombat = SystemAPI.GetSingleton<WaveStateCombatComponent>().Value;
            int waveStatePrep = SystemAPI.GetSingleton<WaveStatePrepComponent>().Value;

            systemState.Dependency = new WaveStateJob
            {
                AliveEnemyCount = _enemyQuery.CalculateEntityCount() ,
                DoAction = doAction ,
                EnemiesKilled = enemiesKilled ,
                EnemiesToKill = enemiesToKill ,
                IsTutorialActive = isTutorialActive ,
                NoAction = noAction ,
                TimerExpired = timerExpired ,
                Wave1Multiplier = wave1Multiplier ,
                Wave2Multiplier = wave2Multiplier ,
                Wave3Multiplier = wave3Multiplier ,
                WavesPerLevel = wavesPerLevel ,
                WaveStateCombat = waveStateCombat ,
                WaveStatePrep = waveStatePrep
            }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct WaveStateJob : IJobEntity
    {
        public int AliveEnemyCount;
        public int DoAction;
        public int EnemiesKilled;
        public int EnemiesToKill;
        public bool IsTutorialActive;
        public int NoAction;
        public float TimerExpired;
        public float Wave1Multiplier;
        public float Wave2Multiplier;
        public float Wave3Multiplier;
        public int WavesPerLevel;
        public int WaveStateCombat;
        public int WaveStatePrep;

        private void Execute(ref TimerComponent timerComponent , ref WaveIndexComponent waveIndexComponent , in WavePrepDurationComponent wavePrepDurationComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool isPrepComplete = waveStateComponent.Value == WaveStatePrep && timerComponent.Value <= TimerExpired && !IsTutorialActive;

            // We only transition to the next wave if we still need more kills for the level.
            bool isLevelOngoing = EnemiesKilled < EnemiesToKill;
            bool isWaveClear = waveStateComponent.Value == WaveStateCombat && waveStockComponent.Value <= NoAction && AliveEnemyCount <= NoAction && isLevelOngoing;

            waveIndexComponent.Value += math.select(NoAction , DoAction , isPrepComplete);
            waveStateComponent.Value = math.select(waveStateComponent.Value , WaveStateCombat , isPrepComplete);
            waveStateComponent.Value = math.select(waveStateComponent.Value , WaveStatePrep , isWaveClear);

            int enemiesNeededForLevel = EnemiesToKill - EnemiesKilled - AliveEnemyCount;

            // Prevent modulo by zero if inspector value is 0
            int safeWavesPerLevel = math.max(DoAction , WavesPerLevel);
            int currentWaveInLevel = (waveIndexComponent.Value - DoAction) % safeWavesPerLevel;
            currentWaveInLevel = math.select(currentWaveInLevel , NoAction , currentWaveInLevel < NoAction);

            float multiplier = math.select(math.select(Wave3Multiplier , Wave2Multiplier , currentWaveInLevel == DoAction) , Wave1Multiplier , currentWaveInLevel == NoAction);

            int calculatedStock = (int)(enemiesNeededForLevel * multiplier);

            // Require at least 1 enemy if the quota is not yet met to prevent softlocks
            int minRequiredStock = math.select(NoAction , DoAction , enemiesNeededForLevel > NoAction);

            // Clamp the final stock between the minimum required and the absolute maximum needed
            int cappedStock = math.clamp(calculatedStock , minRequiredStock , enemiesNeededForLevel);

            waveStockComponent.Value = math.select(waveStockComponent.Value , cappedStock , isPrepComplete);
            timerComponent.Value = math.select(timerComponent.Value , wavePrepDurationComponent.Value , isWaveClear);
        }
    }
}