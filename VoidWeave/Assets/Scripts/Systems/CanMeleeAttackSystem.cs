namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CollisionSystem))]
    public partial struct CanMeleeAttackSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            JobHandle jobHandle = new CanMeleeAttackJob { ECB = ecb }.ScheduleParallel(state.Dependency);
            
            state.Dependency = new CannotMeleeAttackJob { ECB = ecb }.ScheduleParallel(jobHandle);
        }
    }

    [BurstCompile]
    [WithAll(typeof(MeleeAttackRateComponent))]
    [WithNone(typeof(CooldownComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CanMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.AddComponent<CanMeleeAttackTag>(sortKey , entity); }
    }

    [BurstCompile]
    [WithAll(typeof(CooldownComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CannotMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.RemoveComponent<CanMeleeAttackTag>(sortKey , entity); }
    }
}