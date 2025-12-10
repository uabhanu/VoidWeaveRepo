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

        // Note: RandomComponent must be 'ref' because NextFloat() modifies its internal state
        private void Execute([EntityIndexInQuery] int entityInQueryIndex , ref EnemySpawnTimerComponent enemySpawnTimerComponent , ref RandomComponent randomComponent , in EnemySpawnRateComponent enemySpawnRateComponent , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemyPrefabComponent enemyPrefabComponent , in LocalTransform localTransform)
        {
            // 1. Countdown
            enemySpawnTimerComponent.EnemySpawnTimer -= DeltaTime;

            // 2. Check Trigger
            float shouldSpawn = math.step(enemySpawnTimerComponent.EnemySpawnTimer , 0f);

            // 3. Reset Timer (Select logic)
            // If spawning, reset to Rate. If not, keep current value.
            enemySpawnTimerComponent.EnemySpawnTimer = math.select(enemySpawnTimerComponent.EnemySpawnTimer , enemySpawnRateComponent.EnemySpawnRate , shouldSpawn > 0.5f);

            // 4. Calculate Spawn Count (0 or 1)
            int spawnCount = (int)shouldSpawn;

            // 5. Loop (Runs 0 times or 1 time)
            for(int i = 0 ; i < spawnCount ; i++)
            {
                // Instantiate
                Entity newEnemy = EntityCommandBuffer.Instantiate(entityInQueryIndex , enemyPrefabComponent.EnemyPrefab);

                // Calculate Random Position on Circle
                // Get a random angle between 0 and 2 PI
                float angle = randomComponent.RandomValue.NextFloat(0f , math.PI * 2);

                // Convert Polar to Cartesian
                float x = math.cos(angle) * enemySpawnRadiusComponent.EnemySpawnRadius;
                float y = math.sin(angle) * enemySpawnRadiusComponent.EnemySpawnRadius;

                float3 spawnOffset = new float3(x , y , 0);
                float3 finalPos = localTransform.Position + spawnOffset;

                // Set Position
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , LocalTransform.FromPosition(finalPos));

                // Initialize Enemy Movement (Move towards center/player?)
                // For now, we just give them a MoveInput. 
                // In the next step (AI), we will make them target the player.
                // We default them to moving slightly so we can see they are alive.
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , new MovementInputComponent { MoveInput = new float2(0 , 0) });
            }
        }
    }
}