namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

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

            // 1. Check Readiness (Open the Gate)
            systemState.Dependency = new CanMeleeAttackJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new CanRangeAttackJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);

            systemState.Dependency = new CannotMeleeAttackJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new CannotRangeAttackJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);

            // 2. Reset Cooldowns (Close the Gate after Attack) - FIXED
            systemState.Dependency = new ResetMeleeAttackCooldownJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new ResetRangedAttackCooldownJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent))]
    [WithNone(typeof(CanMeleeAttackTag))]
    public partial struct CanMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0) ; i++) { ECB.AddComponent<CanMeleeAttackTag>(entityIndexInQuery , entity); }
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CannotMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer > 0) ; i++) { ECB.RemoveComponent<CanMeleeAttackTag>(entityIndexInQuery , entity); }
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(HasTargetTag))]
    [WithNone(typeof(CanShootTag))]
    public partial struct CanRangeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0) ; i++) { ECB.AddComponent<CanShootTag>(entityIndexInQuery , entity); }
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanShootTag))]
    public partial struct CannotRangeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer > 0) ; i++) { ECB.RemoveComponent<CanShootTag>(entityIndexInQuery , entity); }
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanMeleeAttackTag))]
    public partial struct ResetMeleeAttackCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in AttackRateComponent attackRate , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = attackRate.AttackRate });
            ECB.RemoveComponent<CanMeleeAttackTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    public partial struct ResetRangedAttackCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in AttackRateComponent attackRate , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            // Reset Timer
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = attackRate.AttackRate });
            ECB.RemoveComponent<CanShootTag>(entityIndexInQuery , entity);
        }
    }
}