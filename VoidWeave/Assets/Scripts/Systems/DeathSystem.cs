namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct DeathSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DeathTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DeathJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(state.Dependency);
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