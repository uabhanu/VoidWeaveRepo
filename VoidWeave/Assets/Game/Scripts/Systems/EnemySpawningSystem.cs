namespace Game.Scripts.Systems
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
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag>().Build());
            systemState.RequireForUpdate<EnemyTypesCountComponent>();
            systemState.RequireForUpdate<InitialBitmaskComponent>();
            systemState.RequireForUpdate<LineEnemyIndexComponent>();
            systemState.RequireForUpdate<MovementNoneComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<RandomRangeStartComponent>();
            systemState.RequireForUpdate<SquareEnemyIndexComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            systemState.RequireForUpdate<TriangleEnemyIndexComponent>();
            systemState.RequireForUpdate<UnlockedEnemiesComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int activeWaveState = SystemAPI.GetSingleton<ActiveWaveStateComponent>().Value;
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int enemyTypesCount = SystemAPI.GetSingleton<EnemyTypesCountComponent>().Value;
            uint initialBitmask = SystemAPI.GetSingleton<InitialBitmaskComponent>().Value;
            float movementNone = SystemAPI.GetSingleton<MovementNoneComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
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
                DoAction = doAction ,
                EnemyTypesCount = enemyTypesCount ,
                EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() ,
                InitialBitmask = initialBitmask ,
                LineEnemyEntity = lineEnemyEntity ,
                LineEnemyIndex = lineEnemyIndex ,
                MovementNone = movementNone ,
                NoAction = noAction ,
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
        public int DoAction;
        public int EnemyTypesCount;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public uint InitialBitmask;
        public Entity LineEnemyEntity;
        public int LineEnemyIndex;
        public float MovementNone;
        public int NoAction;
        public int PlayerCount;
        public int RandomRangeStartValue;
        public Entity SquareEnemyEntity;
        public int SquareEnemyIndex;
        public float TimerExpired;
        public Entity TriangleEnemyEntity;
        public int TriangleEnemyIndex;
        public uint UnlockedEnemiesBitmask;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , ref RandomSeedComponent randomSeedComponent , in SpawnRadiusComponent spawnRadiusComponent , in SpawnRateComponent spawnRateComponent , ref TimerComponent timerComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool canSpawn = timerComponent.Value <= TimerExpired && PlayerCount > NoAction && waveStateComponent.Value == ActiveWaveState && waveStockComponent.Value > NoAction;

            for(var i = 0 ; i < math.select(NoAction , DoAction , canSpawn) ; i++)
            {
                int enemyTypeIndex = randomSeedComponent.Value.NextInt(RandomRangeStartValue , EnemyTypesCount);
                bool isUnlocked = (UnlockedEnemiesBitmask & (InitialBitmask << enemyTypeIndex)) != NoAction;
                enemyTypeIndex = math.select(LineEnemyIndex , enemyTypeIndex , isUnlocked);

                Entity enemyEntityToSpawn = enemyTypeIndex == LineEnemyIndex ? LineEnemyEntity : enemyTypeIndex == SquareEnemyIndex ? SquareEnemyEntity : TriangleEnemyEntity;
                Entity newEnemyEntity = EntityCommandBufferParallelWriter.Instantiate(entityInQueryIndex , enemyEntityToSpawn);

                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , LocalTransform.FromPosition(localTransform.Position + new float3(randomSeedComponent.Value.NextFloat2Direction() * spawnRadiusComponent.Value , MovementNone)));
                EntityCommandBufferParallelWriter.AddComponent<EnemyJustSpawnedTag>(entityInQueryIndex , newEnemyEntity);

                for(var k = 0 ; k < math.select(NoAction , DoAction , enemyTypeIndex == LineEnemyIndex) ; k++) EntityCommandBufferParallelWriter.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemyEntity);

                for(var k = 0 ; k < math.select(NoAction , DoAction , enemyTypeIndex == TriangleEnemyIndex) ; k++) EntityCommandBufferParallelWriter.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemyEntity);

                for(var k = 0 ; k < math.select(NoAction , DoAction , enemyTypeIndex == SquareEnemyIndex) ; k++) EntityCommandBufferParallelWriter.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemyEntity);
            }

            timerComponent.Value = math.select(timerComponent.Value , spawnRateComponent.Value , canSpawn);
            waveStockComponent.Value -= math.select(NoAction , DoAction , canSpawn);
        }
    }
}