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
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            // 1. Check Readiness (Open the Gate)
            systemState.Dependency = new CanMeleeAttackJob { DoAction = doAction , ECB = ecb , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new CanRangeAttackJob { DoAction = doAction , ECB = ecb , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);

            systemState.Dependency = new CannotMeleeAttackJob { DoAction = doAction , ECB = ecb , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new CannotRangeAttackJob { DoAction = doAction , ECB = ecb , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);

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
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int NoAction;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(NoAction , DoAction , cooldownComponent.Value <= TimerExpired) ; i++) ECB.AddComponent<CanMeleeAttackTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CannotMeleeAttackJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int NoAction;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(NoAction , DoAction , cooldownComponent.Value > TimerExpired) ; i++) ECB.RemoveComponent<CanMeleeAttackTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(HasTargetTag))]
    [WithNone(typeof(CanShootTag))]
    public partial struct CanRangeAttackJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int NoAction;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(NoAction , DoAction , cooldownComponent.Value <= TimerExpired) ; i++) ECB.AddComponent<CanShootTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanShootTag))]
    public partial struct CannotRangeAttackJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int NoAction;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(NoAction , DoAction , cooldownComponent.Value > TimerExpired) ; i++) ECB.RemoveComponent<CanShootTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanMeleeAttackTag))]
    public partial struct ResetMeleeAttackCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in AttackRateComponent attackRate , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Value = attackRate.Value });
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
            // Reset Entity
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Value = attackRate.Value });
            ECB.RemoveComponent<CanShootTag>(entityIndexInQuery , entity);
        }
    }
}