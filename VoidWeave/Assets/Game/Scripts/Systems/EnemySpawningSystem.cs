namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(AdvanceLevelSystem))]
    [UpdateAfter(typeof(TimerSystem))]
    [UpdateAfter(typeof(WaveStateSystem))]
    public partial struct EnemySpawningSystem : ISystem
    {
        private EntityQuery _playerQuery;
        
        public void OnCreate(ref SystemState systemState)
        {
            _playerQuery = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build();
            
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
        
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency = new EnemySpawnJob
            {
                ActiveWaveState = SystemAPI.GetSingleton<ActiveWaveStateComponent>().Value ,
                BoundaryX = SystemAPI.GetSingleton<ScreenBoundaryXComponent>().Value ,
                BoundaryY = SystemAPI.GetSingleton<ScreenBoundaryYComponent>().Value ,
                EnemyTypesCount = SystemAPI.GetSingleton<EnemyTypesCountComponent>().Value ,
                ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() ,
                InitialBitmask = SystemAPI.GetSingleton<InitialBitmaskComponent>().Value ,
                LineEnemyEntity = SystemAPI.GetComponent<LineEnemyEntityComponent>(SystemAPI.GetSingletonEntity<EnemySpawnerTag>()).Entity ,
                LineEnemyIndex = SystemAPI.GetSingleton<LineEnemyIndexComponent>().Value ,
                PlayerCount = _playerQuery.CalculateEntityCount() ,
                RandomRangeStartValue = SystemAPI.GetSingleton<RandomRangeStartComponent>().Value ,
                SquareEnemyEntity = SystemAPI.GetComponent<SquareEnemyEntityComponent>(SystemAPI.GetSingletonEntity<EnemySpawnerTag>()).Entity ,
                SquareEnemyIndex = SystemAPI.GetSingleton<SquareEnemyIndexComponent>().Value ,
                TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value ,
                TriangleEnemyEntity = SystemAPI.GetComponent<TriangleEnemyEntityComponent>(SystemAPI.GetSingletonEntity<EnemySpawnerTag>()).Entity ,
                TriangleEnemyIndex = SystemAPI.GetSingleton<TriangleEnemyIndexComponent>().Value ,
                UnlockedEnemiesBitmask = SystemAPI.GetSingleton<UnlockedEnemiesComponent>().Value
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
        public EntityCommandBuffer.ParallelWriter ECB;
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
                Entity newEnemyEntity = ECB.Instantiate(entityInQueryIndex , enemyEntityToSpawn);
                
                ECB.SetComponentEnabled<SpawningVfxTag>(entityInQueryIndex , newEnemyEntity , true);

                float randomX = randomSeedComponent.Value.NextFloat(-BoundaryX , BoundaryX);
                float randomY = randomSeedComponent.Value.NextFloat(-BoundaryY , BoundaryY);
                float3 spawnPosition = new float3(randomX , randomY , 0);

                ECB.SetComponent(entityInQueryIndex , newEnemyEntity , LocalTransform.FromPosition(spawnPosition).WithScale(0.0f));
            }

            timerComponent.Value = math.select(timerComponent.Value , spawnRateComponent.Value , canSpawn);
            waveStockComponent.Value -= math.select(0 , 1 , canSpawn);
        }
    }
}