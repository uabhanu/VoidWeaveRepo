namespace Game.Scripts.Systems
{
    using Game.Scripts.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct DeathSystem : ISystem
    {
        private EntityQuery _dyingEnemyEntityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _dyingEnemyEntityQuery = SystemAPI.QueryBuilder().WithAll<DeathTag , EnemyTag>().Build();

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<DeathTag>();
            systemState.RequireForUpdate<EnemiesKilledComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int killedCount = _dyingEnemyEntityQuery.CalculateEntityCount();

            SystemAPI.GetSingletonRW<EnemiesKilledComponent>().ValueRW.Value += killedCount;

            systemState.Dependency = new DeathJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(systemState.Dependency);
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