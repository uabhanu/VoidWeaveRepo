using Components;

namespace Systems
{
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemySpawningSystem))] // Run BEFORE spawning so the state is fresh
    public partial struct WaveSystem : ISystem
    {
        private const float PREP_TIME = 30.0f;
        private const int BASE_ENEMIES = 10;
        private const int ENEMY_INCREMENT = 5;

        private EntityQuery _enemyQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveStateComponent>();

            // Query to count how many enemies are currently alive (Team 1)
            // We use SeekerTag to identify active enemy units
            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag , TeamComponent>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Efficiently count entities on the Main Thread to detect if wave is cleared
            int aliveEnemyCount = _enemyQuery.CalculateEntityCount();

            new WaveLogicJob { DeltaTime = deltaTime , AliveEnemyCount = aliveEnemyCount , PrepDuration = PREP_TIME , BaseEnemies = BASE_ENEMIES , Increment = ENEMY_INCREMENT }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct WaveLogicJob : IJobEntity
    {
        public float DeltaTime;
        public int AliveEnemyCount;
        public float PrepDuration;
        public int BaseEnemies;
        public int Increment;

        private void Execute(ref WaveIndexComponent waveIndexComponent , ref WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent , ref WaveTimerComponent waveTimerComponent)
        {
            // Decrement Timer
            waveTimerComponent.Timer -= DeltaTime;

            // --- CONDITIONS (Branchless) ---

            // Current State
            float isPrep = math.step(waveStateComponent.State , 0.1f); // 1.0 if State == 0
            float isCombat = math.step(0.9f , waveStateComponent.State); // 1.0 if State == 1

            // Triggers
            float timerExpired = math.step(waveTimerComponent.Timer , 0f);
            float stockEmpty = math.step(waveStockComponent.Stock , 0);
            float enemiesDead = math.step(AliveEnemyCount , 0);

            // Transitions
            // Start Combat if: In Prep AND Timer finished
            float startCombat = isPrep * timerExpired;

            // Start Prep if: In Combat AND No Stock left AND All Enemies dead
            float startPrep = isCombat * stockEmpty * enemiesDead;

            // --- UPDATE DATA ---

            // Increment Wave Index (Only when starting Combat)
            waveIndexComponent.Index += (int)startCombat;

            // Calculate New Stock Size (Difficulty Scaling)
            int newStockAmount = BaseEnemies + (waveIndexComponent.Index * Increment);

            // Update Stock
            // If Starting Combat -> Set to New Amount
            // If Starting Prep -> Set to 0 (Clean up)
            // Else -> Keep current value
            int currentStock = waveStockComponent.Stock;
            currentStock = math.select(currentStock , newStockAmount , startCombat > 0.5f);
            currentStock = math.select(currentStock , 0 , startPrep > 0.5f);
            waveStockComponent.Stock = currentStock;

            // Update Timer
            // If Starting Prep -> Reset to 30s
            // Else -> Keep current (Combat doesn't use the waveTimerComponent, but we let it run down)
            waveTimerComponent.Timer = math.select(waveTimerComponent.Timer , PrepDuration , startPrep > 0.5f);

            // Update Phase State
            // If StartCombat -> 1
            // If StartPrep -> 0
            int nextState = waveStateComponent.State;
            nextState = math.select(nextState , 1 , startCombat > 0.5f);
            nextState = math.select(nextState , 0 , startPrep > 0.5f);
            waveStateComponent.State = nextState;
        }
    }
}