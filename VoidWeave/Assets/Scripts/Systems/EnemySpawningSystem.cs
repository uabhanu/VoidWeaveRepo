namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(LevelProgressionSystem))]
    [UpdateAfter(typeof(TimerSystem))]
    [UpdateAfter(typeof(WaveStateSystem))]
    public partial struct EnemySpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerTag>();

            Entity lineEnemyEntity = SystemAPI.GetComponent<LineEnemyEntityComponent>(spawnerEntity).Entity;
            Entity squareEnemyEntity = SystemAPI.GetComponent<SquareEnemyEntityComponent>(spawnerEntity).Entity;
            Entity triangleEnemyEntity = SystemAPI.GetComponent<TriangleEnemyEntityComponent>(spawnerEntity).Entity;

            systemState.Dependency = new EnemySpawnJob { EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , LineEnemyEntity = lineEnemyEntity , PlayerCount = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build().CalculateEntityCount() , SquareEnemyEntity = squareEnemyEntity , TriangleEnemyEntity = triangleEnemyEntity , }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public Entity LineEnemyEntity;
        public int PlayerCount;
        public Entity SquareEnemyEntity;
        public Entity TriangleEnemyEntity;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , in LocalTransform localTransform , ref RandomComponent randomComponent , ref TimerComponent timerComponent , in UnlockedEnemiesComponent unlockedEnemiesComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool canSpawn = timerComponent.Timer <= 0f && PlayerCount > 0 && waveStateComponent.State == 1 && waveStockComponent.Stock > 0;

            for(int i = 0 ; i < math.select(0 , 1 , canSpawn) ; i++)
            {
                int enemyTypeIndex = randomComponent.Random.NextInt(0 , 3);
                bool isUnlocked = (unlockedEnemiesComponent.UnlockedEnemiesBitmask & (1u << enemyTypeIndex)) != 0;
                enemyTypeIndex = math.select(0 , enemyTypeIndex , isUnlocked);

                Entity enemyEntityToSpawn = enemyTypeIndex == 1 ? LineEnemyEntity : enemyTypeIndex == 2 ? SquareEnemyEntity : TriangleEnemyEntity;
                Entity newEnemyEntity = EntityCommandBufferParallelWriter.Instantiate(entityInQueryIndex , enemyEntityToSpawn);

                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , LocalTransform.FromPosition(localTransform.Position + new float3(randomComponent.Random.NextFloat2Direction() * enemySpawnRadiusComponent.Radius , 0f)));
                EntityCommandBufferParallelWriter.AddComponent<EnemyJustSpawnedTag>(entityInQueryIndex , newEnemyEntity);

                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == 0) ; k++) { EntityCommandBufferParallelWriter.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemyEntity); }
                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == 1) ; k++) { EntityCommandBufferParallelWriter.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemyEntity); }
                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == 2) ; k++) { EntityCommandBufferParallelWriter.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemyEntity); }
            }

            timerComponent.Timer = math.select(timerComponent.Timer , enemySpawnRateComponent.Rate , canSpawn);
            waveStockComponent.Stock -= math.select(0 , 1 , canSpawn);
        }
    }
}