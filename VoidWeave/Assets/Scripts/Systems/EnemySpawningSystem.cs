namespace Systems
{
    using Components;
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
            state.Dependency = new EnemySpawnJob { DeltaTime = SystemAPI.Time.DeltaTime , EntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        
        private void Execute(in EnemyEntityComponent enemyEntityComponent , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , ref EnemySpawnTimerComponent enemySpawnTimerComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , ref RandomComponent randomComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            enemySpawnTimerComponent.Timer -= DeltaTime;
            
            for(int i = 0 ; i < math.select(0 , 1 , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0) ; i++)
            {
                EntityCommandBuffer.Instantiate(entityInQueryIndex , enemyEntityComponent.Entity);
                
                EntityCommandBuffer.SetComponent(entityInQueryIndex , enemyEntityComponent.Entity , LocalTransform.FromPosition(localTransform.Position + new float3(randomComponent.Random.NextFloat2Direction() * enemySpawnRadiusComponent.Radius , 0f)));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , enemyEntityComponent.Entity , new MovementInputComponent { Input = float2.zero });
            }
            
            waveStockComponent.Stock -= math.select(0 , 1 , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0);
            enemySpawnTimerComponent.Timer = math.select(enemySpawnTimerComponent.Timer , enemySpawnRateComponent.Rate , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0);
        }
    }
}