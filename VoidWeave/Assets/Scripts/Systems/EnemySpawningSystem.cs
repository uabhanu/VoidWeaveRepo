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
            systemState.RequireForUpdate<ActiveWaveStateComponent>();
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag>().Build());
            systemState.RequireForUpdate<EnemyTypesCountComponent>();
            systemState.RequireForUpdate<InitialBitmaskComponent>();
            systemState.RequireForUpdate<LineEnemyIndexComponent>();
            systemState.RequireForUpdate<RandomRangeStartComponent>();
            systemState.RequireForUpdate<SquareEnemyIndexComponent>();
            systemState.RequireForUpdate<TriangleEnemyIndexComponent>();
            systemState.RequireForUpdate<UnlockedEnemiesComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int activeWaveState = SystemAPI.GetSingleton<ActiveWaveStateComponent>().ActiveWaveState;
            
            int enemyTypesCount = SystemAPI.GetSingleton<EnemyTypesCountComponent>().EnemyTypesCount;
            
            uint initialBitmask = SystemAPI.GetSingleton<InitialBitmaskComponent>().InitialBitmask;

            int randomRangeStartValue = SystemAPI.GetSingleton<RandomRangeStartComponent>().RandomRangeStartValue;
            
            uint unlockedEnemiesBitmask = SystemAPI.GetSingleton<UnlockedEnemiesComponent>().UnlockedEnemiesBitmask;

            Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerTag>();

            Entity lineEnemyEntity = SystemAPI.GetComponent<LineEnemyEntityComponent>(spawnerEntity).Entity;
            int lineEnemyIndex = SystemAPI.GetSingleton<LineEnemyIndexComponent>().LineEnemyIndex;

            Entity squareEnemyEntity = SystemAPI.GetComponent<SquareEnemyEntityComponent>(spawnerEntity).Entity;
            int squareEnemyIndex = SystemAPI.GetSingleton<SquareEnemyIndexComponent>().SquareEnemyIndex;

            Entity triangleEnemyEntity = SystemAPI.GetComponent<TriangleEnemyEntityComponent>(spawnerEntity).Entity;
            int triangleEnemyIndex = SystemAPI.GetSingleton<TriangleEnemyIndexComponent>().TriangleEnemyIndex;

            systemState.Dependency = new EnemySpawnJob
            {
                ActiveWaveState = activeWaveState ,
                EnemyTypesCount = enemyTypesCount ,
                EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() ,
                InitialBitmask = initialBitmask ,
                LineEnemyEntity = lineEnemyEntity ,
                LineEnemyIndex = lineEnemyIndex ,
                PlayerCount = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build().CalculateEntityCount() ,
                RandomRangeStartValue = randomRangeStartValue ,
                SquareEnemyEntity = squareEnemyEntity ,
                SquareEnemyIndex = squareEnemyIndex ,
                TriangleEnemyEntity = triangleEnemyEntity ,
                TriangleEnemyIndex = triangleEnemyIndex ,
                UnlockedEnemiesBitmask = unlockedEnemiesBitmask
            }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public int ActiveWaveState;
        public int EnemyTypesCount;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public uint InitialBitmask;
        public Entity LineEnemyEntity;
        public int LineEnemyIndex;
        public int PlayerCount;
        public int RandomRangeStartValue;
        public Entity SquareEnemyEntity;
        public int SquareEnemyIndex;
        public Entity TriangleEnemyEntity;
        public int TriangleEnemyIndex;
        public uint UnlockedEnemiesBitmask;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , ref RandomSeedComponent randomSeedComponent , in SpawnRadiusComponent spawnRadiusComponent , in SpawnRateComponent spawnRateComponent , ref TimerComponent timerComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool canSpawn = timerComponent.Timer <= 0f && PlayerCount > 0 && waveStateComponent.State == ActiveWaveState && waveStockComponent.Stock > 0;

            for(int i = 0 ; i < math.select(0 , 1 , canSpawn) ; i++)
            {
                int enemyTypeIndex = randomSeedComponent.RandomSeed.NextInt(RandomRangeStartValue , EnemyTypesCount);
                bool isUnlocked = (UnlockedEnemiesBitmask & (InitialBitmask << enemyTypeIndex)) != 0;
                enemyTypeIndex = math.select(TriangleEnemyIndex , enemyTypeIndex , isUnlocked);

                Entity enemyEntityToSpawn = enemyTypeIndex == LineEnemyIndex ? LineEnemyEntity : enemyTypeIndex == SquareEnemyIndex ? SquareEnemyEntity : TriangleEnemyEntity;
                Entity newEnemyEntity = EntityCommandBufferParallelWriter.Instantiate(entityInQueryIndex , enemyEntityToSpawn);

                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , LocalTransform.FromPosition(localTransform.Position + new float3(randomSeedComponent.RandomSeed.NextFloat2Direction() * spawnRadiusComponent.Radius , 0f)));
                EntityCommandBufferParallelWriter.AddComponent<EnemyJustSpawnedTag>(entityInQueryIndex , newEnemyEntity);

                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == TriangleEnemyIndex) ; k++) { EntityCommandBufferParallelWriter.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemyEntity); }
                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == LineEnemyIndex) ; k++) { EntityCommandBufferParallelWriter.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemyEntity); }
                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == SquareEnemyIndex) ; k++) { EntityCommandBufferParallelWriter.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemyEntity); }
            }

            timerComponent.Timer = math.select(timerComponent.Timer , spawnRateComponent.Rate , canSpawn);
            waveStockComponent.Stock -= math.select(0 , 1 , canSpawn);
        }
    }
}