namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(CampaignProgressionSystem))]
    [UpdateAfter(typeof(TimerSystem))]
    [UpdateAfter(typeof(WaveStateSystem))]
    public partial struct EnemySpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<ActiveWaveStateComponent>();
            systemState.RequireForUpdate<CameraOrthographicSizeComponent>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag>().Build());
            systemState.RequireForUpdate<EnemyTypesCountComponent>();
            systemState.RequireForUpdate<InitialBitmaskComponent>();
            systemState.RequireForUpdate<LineEnemyIndexComponent>();
            systemState.RequireForUpdate<RandomRangeStartComponent>();
            systemState.RequireForUpdate<ScreenBoundaryXComponent>();
            systemState.RequireForUpdate<ScreenBoundaryYComponent>();
            systemState.RequireForUpdate<SquareEnemyIndexComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            systemState.RequireForUpdate<TriangleEnemyIndexComponent>();
            systemState.RequireForUpdate<UnlockedEnemiesComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int activeWaveState = SystemAPI.GetSingleton<ActiveWaveStateComponent>().Value;
            float boundaryX = SystemAPI.GetSingleton<ScreenBoundaryXComponent>().Value;
            float boundaryY = SystemAPI.GetSingleton<ScreenBoundaryYComponent>().Value;
            int enemyTypesCount = SystemAPI.GetSingleton<EnemyTypesCountComponent>().Value;
            uint initialBitmask = SystemAPI.GetSingleton<InitialBitmaskComponent>().Value;
            int randomRangeStartValue = SystemAPI.GetSingleton<RandomRangeStartComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;
            uint unlockedEnemiesBitmask = SystemAPI.GetSingleton<UnlockedEnemiesComponent>().Value;
            Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerTag>();

            Entity lineEnemyEntity = SystemAPI.GetComponent<LineEnemyEntityComponent>(spawnerEntity).Entity;
            int lineEnemyIndex = SystemAPI.GetSingleton<LineEnemyIndexComponent>().Value;

            Entity squareEnemyEntity = SystemAPI.GetComponent<SquareEnemyEntityComponent>(spawnerEntity).Entity;
            int squareEnemyIndex = SystemAPI.GetSingleton<SquareEnemyIndexComponent>().Value;

            Entity triangleEnemyEntity = SystemAPI.GetComponent<TriangleEnemyEntityComponent>(spawnerEntity).Entity;
            int triangleEnemyIndex = SystemAPI.GetSingleton<TriangleEnemyIndexComponent>().Value;

            systemState.Dependency = new EnemySpawnJob
            {
                ActiveWaveState = activeWaveState ,
                BoundaryX = boundaryX ,
                BoundaryY = boundaryY ,
                EnemyTypesCount = enemyTypesCount ,
                EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() ,
                InitialBitmask = initialBitmask ,
                LineEnemyEntity = lineEnemyEntity ,
                LineEnemyIndex = lineEnemyIndex ,
                PlayerCount = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build().CalculateEntityCount() ,
                RandomRangeStartValue = randomRangeStartValue ,
                SquareEnemyEntity = squareEnemyEntity ,
                SquareEnemyIndex = squareEnemyIndex ,
                TimerExpired = timerExpired ,
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
        public float BoundaryX;
        public float BoundaryY;
        public int EnemyTypesCount;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public uint InitialBitmask;
        public Entity LineEnemyEntity;
        public int LineEnemyIndex;
        public int PlayerCount;
        public int RandomRangeStartValue;
        public Entity SquareEnemyEntity;
        public int SquareEnemyIndex;
        public float TimerExpired;
        public Entity TriangleEnemyEntity;
        public int TriangleEnemyIndex;
        public uint UnlockedEnemiesBitmask;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , ref RandomSeedComponent randomSeedComponent , in SpawnRateComponent spawnRateComponent , ref TimerComponent timerComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool canSpawn = timerComponent.Value <= TimerExpired && PlayerCount > 0 && waveStateComponent.Value == ActiveWaveState && waveStockComponent.Value > 0;

            for(var i = 0 ; i < math.select(0 , 1 , canSpawn) ; i++)
            {
                int enemyTypeIndex = randomSeedComponent.Value.NextInt(RandomRangeStartValue , EnemyTypesCount);
                bool isUnlocked = (UnlockedEnemiesBitmask & (InitialBitmask << enemyTypeIndex)) != 0;
                enemyTypeIndex = math.select(LineEnemyIndex , enemyTypeIndex , isUnlocked);

                Entity enemyEntityToSpawn = enemyTypeIndex == LineEnemyIndex ? LineEnemyEntity : enemyTypeIndex == SquareEnemyIndex ? SquareEnemyEntity : TriangleEnemyEntity;
                Entity newEnemyEntity = EntityCommandBufferParallelWriter.Instantiate(entityInQueryIndex , enemyEntityToSpawn);

                EntityCommandBufferParallelWriter.AddComponent<SpawningTag>(entityInQueryIndex , newEnemyEntity);
                EntityCommandBufferParallelWriter.SetComponentEnabled<SpawningTag>(entityInQueryIndex , newEnemyEntity , true);

                float randomX = randomSeedComponent.Value.NextFloat(-BoundaryX , BoundaryX);
                float randomY = randomSeedComponent.Value.NextFloat(-BoundaryY , BoundaryY);
                float3 spawnPosition = new float3(randomX , randomY , 0);

                EntityCommandBufferParallelWriter.AddComponent<EnemyJustSpawnedTag>(entityInQueryIndex , newEnemyEntity);
                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , LocalTransform.FromPosition(spawnPosition).WithScale(0.0f));

                for(var k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == LineEnemyIndex) ; k++) EntityCommandBufferParallelWriter.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemyEntity);
                for(var k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == TriangleEnemyIndex) ; k++) EntityCommandBufferParallelWriter.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemyEntity);
                for(var k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == SquareEnemyIndex) ; k++) EntityCommandBufferParallelWriter.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemyEntity);
            }

            timerComponent.Value = math.select(timerComponent.Value , spawnRateComponent.Value , canSpawn);
            waveStockComponent.Value -= math.select(0 , 1 , canSpawn);
        }
    }
}