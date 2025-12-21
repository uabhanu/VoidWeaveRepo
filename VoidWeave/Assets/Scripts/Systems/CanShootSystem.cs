namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ShootingSystem))] // Ensure permissions are set before shooting
    public partial struct CanShootSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            JobHandle grantHandle = new GrantPermissionJob { ECB = ecb }.ScheduleParallel(state.Dependency);
            
            JobHandle revokeCooldownHandle = new RevokePermissionCooldownJob { ECB = ecb }.ScheduleParallel(grantHandle);
            
            state.Dependency = new RevokePermissionNoTargetJob { ECB = ecb }.ScheduleParallel(revokeCooldownHandle);
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(HasTargetTag))]
    [WithNone(typeof(CooldownComponent) , typeof(CanShootTag))]
    public partial struct GrantPermissionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.AddComponent<CanShootTag>(sortKey , entity); }
    }
    
    [BurstCompile]
    [WithAll(typeof(CanShootTag) , typeof(CooldownComponent))]
    public partial struct RevokePermissionCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.RemoveComponent<CanShootTag>(sortKey , entity); }
    }
    
    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    [WithNone(typeof(HasTargetTag))]
    public partial struct RevokePermissionNoTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.RemoveComponent<CanShootTag>(sortKey , entity); }
    }
}