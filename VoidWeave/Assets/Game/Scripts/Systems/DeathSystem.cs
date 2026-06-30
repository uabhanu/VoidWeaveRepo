namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct DeathSystem : ISystem
    {
        private EntityQuery _dyingEnemyEntityQuery;
        
        public void OnCreate(ref SystemState systemState)
        {
            _dyingEnemyEntityQuery = SystemAPI.QueryBuilder().WithAll<DeathTag , EnemyTag>().Build();

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<DeathTag>();
            systemState.RequireForUpdate<EnemiesKilledComponent>();
        }
        
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            
            int killedCount = _dyingEnemyEntityQuery.CalculateEntityCount();

            SystemAPI.GetSingletonRW<EnemiesKilledComponent>().ValueRW.Value += killedCount;

            systemState.Dependency = new DeathJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(DeathTag))]
    public partial struct DeathJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int index) { ECB.DestroyEntity(index , entity); }
    }
}