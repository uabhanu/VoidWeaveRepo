namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CooldownSystem))]
    public partial struct AttackReadinessSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            
            new CanMeleeAttackJob { ECB = ecb }.ScheduleParallel();
            new CanShootJob { ECB = ecb }.ScheduleParallel();
            
            new CannotMeleeAttackJob { ECB = ecb }.ScheduleParallel();
            new CannotShootJob { ECB = ecb }.ScheduleParallel();
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(MeleeAttackRateComponent))]
    [WithNone(typeof(CooldownComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CanMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery) { ECB.AddComponent<CanMeleeAttackTag>(entityIndexInQuery , entity); }
    }
    
    [BurstCompile]
    [WithAll(typeof(FireRateComponent) , typeof(HasTargetTag))]
    [WithNone(typeof(CooldownComponent) , typeof(CanShootTag))]
    public partial struct CanShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery) { ECB.AddComponent<CanShootTag>(entityIndexInQuery , entity); }
    }

    [BurstCompile]
    [WithAll(typeof(MeleeAttackRateComponent) , typeof(CooldownComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CannotMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.RemoveComponent<CanMeleeAttackTag>(sortKey , entity); }
    }

    [BurstCompile]
    [WithAll(typeof(FireRateComponent) , typeof(CooldownComponent) , typeof(CanShootTag))]
    public partial struct CannotShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.RemoveComponent<CanShootTag>(sortKey , entity); }
    }
}