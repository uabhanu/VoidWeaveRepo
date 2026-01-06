namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WaveSystem))]
    [UpdateAfter(typeof(LevelProgressionSystem))]
    public partial struct EnemySpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<EnemySpawnerTag>();
            systemState.RequireForUpdate<UnlockedEnemiesComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new EnemySpawnJob { DeltaTime = SystemAPI.Time.DeltaTime , EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , PlayerCount = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build().CalculateEntityCount() , }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public int PlayerCount;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , ref EnemySpawnTimerComponent enemySpawnTimerComponent , in LineEnemyEntityComponent lineEnemyEntityComponent , in LocalTransform localTransform , ref RandomComponent randomComponent , in SquareEnemyEntityComponent squareEnemyEntityComponent , in TriangleEnemyEntityComponent triangleEnemyEntityComponent , in UnlockedEnemiesComponent unlockedEnemiesComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            enemySpawnTimerComponent.Timer -= DeltaTime;

            bool canSpawn = enemySpawnTimerComponent.Timer <= 0f && PlayerCount > 0 && waveStateComponent.State == 1 && waveStockComponent.Stock > 0;

            for(int i = 0 ; i < math.select(0 , 1 , canSpawn) ; i++)
            {
                int selection = randomComponent.Random.NextInt(0 , 3);

                // Check if the selected enemy is unlocked
                bool isUnlocked = (unlockedEnemiesComponent.UnlockedEnemiesBitmask & (1u << selection)) != 0;

                // If NOT unlocked, force selection to 0 (Triangle)
                selection = math.select(0 , selection , isUnlocked);

                Entity newEnemy = EntityCommandBufferParallelWriter.Instantiate(entityInQueryIndex , selection == 1 ? lineEnemyEntityComponent.Entity : (selection == 2 ? squareEnemyEntityComponent.Entity : triangleEnemyEntityComponent.Entity));

                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemy , LocalTransform.FromPosition(localTransform.Position + new float3(randomComponent.Random.NextFloat2Direction() * enemySpawnRadiusComponent.Radius , 0f)));
                
                for(int k = 0 ; k < math.select(0 , 1 , selection == 0) ; k++) { EntityCommandBufferParallelWriter.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemy); }
                for(int k = 0 ; k < math.select(0 , 1 , selection == 1) ; k++) { EntityCommandBufferParallelWriter.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemy); }
                for(int k = 0 ; k < math.select(0 , 1 , selection == 2) ; k++) { EntityCommandBufferParallelWriter.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemy); }
            }

            enemySpawnTimerComponent.Timer = math.select(enemySpawnTimerComponent.Timer , enemySpawnRateComponent.Rate , canSpawn);
            waveStockComponent.Stock -= math.select(0 , 1 , canSpawn);
        }
    }
}