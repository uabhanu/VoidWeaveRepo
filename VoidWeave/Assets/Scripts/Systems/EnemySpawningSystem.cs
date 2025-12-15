namespace Systems
{
    using Gameplay;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemySpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EnemySpawnerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new EnemySpawnJob { DeltaTime = deltaTime , EntityCommandBuffer = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        
        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in EnemyEntityComponent enemyEntityComponent , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , ref EnemySpawnTimerComponent enemySpawnTimerComponent , in LocalTransform localTransform , ref RandomComponent randomComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            enemySpawnTimerComponent.EnemySpawnTimer -= DeltaTime;

            // --- CHECKS ---

            // 1. Timer Ready?
            float isTimerReady = math.step(enemySpawnTimerComponent.EnemySpawnTimer , 0f);

            // 2. Is Combat Phase? (State == 1)
            float isCombat = math.step(0.9f , waveStateComponent.WaveState);

            // 3. Is Stock Available? (Value >= 1)
            float hasStock = math.step(1 , waveStockComponent.WaveStock);

            // Combine triggers: All must be true (1.0) to spawn
            float shouldSpawn = isTimerReady * isCombat * hasStock;

            // --- EXECUTE ---

            // Reset Timer if we spawned
            enemySpawnTimerComponent.EnemySpawnTimer = math.select(enemySpawnTimerComponent.EnemySpawnTimer , enemySpawnRateComponent.EnemySpawnRate , shouldSpawn > 0.5f);

            int spawnCount = (int)shouldSpawn;

            // Decrement Stock
            waveStockComponent.WaveStock -= spawnCount;

            for(int i = 0 ; i < spawnCount ; i++)
            {
                Entity newEnemy = EntityCommandBuffer.Instantiate(entityInQueryIndex , enemyEntityComponent.EnemyEntity);

                // Random Position Logic
                float angle = randomComponent.RandomValue.NextFloat(0f , math.PI * 2);
                float x = math.cos(angle) * enemySpawnRadiusComponent.EnemySpawnRadius;
                float y = math.sin(angle) * enemySpawnRadiusComponent.EnemySpawnRadius;
                float3 spawnOffset = new float3(x , y , 0);
                float3 finalPos = localTransform.Position + spawnOffset;

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , LocalTransform.FromPosition(finalPos));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , new MovementInputComponent { MoveInput = new float2(0 , 0) });
            }
        }
    }
}