namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(CooldownSystem))]
    public partial struct AttackReadinessSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency = new CanMeleeAttackJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new CanRangeAttackJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency);

            systemState.Dependency = new CannotMeleeAttackJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new CannotRangeAttackJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency);
            
            systemState.Dependency = new ResetMeleeAttackCooldownJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new ResetRangedAttackCooldownJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof (CanMeleeAttackTag))]
    [WithNone(typeof(DeployingTurretTag))]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct CanMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(0 , 1 , cooldownComponent.Value <= TimerExpired) ; i++) ECB.SetComponentEnabled<CanMeleeAttackTag>(entityIndexInQuery , entity , true);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanMeleeAttackTag))]
    public partial struct CannotMeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(0 , 1 , cooldownComponent.Value > TimerExpired) ; i++) ECB.SetComponentEnabled<CanMeleeAttackTag>(entityIndexInQuery , entity , false);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanRangeAttackTag) , typeof(HasTargetTag) , typeof(RotationCompleteTag))]
    [WithNone(typeof(DeployingTurretTag))]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct CanRangeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(0 , 1 , cooldownComponent.Value <= TimerExpired) ; i++) ECB.SetComponentEnabled<CanRangeAttackTag>(entityIndexInQuery , entity , true);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AttackRateComponent) , typeof(CanRangeAttackTag))]
    public partial struct CannotRangeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float TimerExpired;

        private void Execute(in CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            for(var i = 0 ; i < math.select(0 , 1 , cooldownComponent.Value > TimerExpired) ; i++) ECB.SetComponentEnabled<CanRangeAttackTag>(entityIndexInQuery , entity , false);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanMeleeAttackTag))]
    public partial struct ResetMeleeAttackCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in AttackRateComponent attackRate , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            ECB.SetComponent(entityIndexInQuery , entity , new CooldownComponent { Value = attackRate.Value });
            ECB.SetComponentEnabled<CooldownComponent>(entityIndexInQuery , entity , true);
            
            ECB.SetComponentEnabled<CanMeleeAttackTag>(entityIndexInQuery , entity , false);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanRangeAttackTag))]
    public partial struct ResetRangedAttackCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in AttackRateComponent attackRate , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            // Reset Entity
            ECB.SetComponent(entityIndexInQuery , entity , new CooldownComponent { Value = attackRate.Value });
            ECB.SetComponentEnabled<CooldownComponent>(entityIndexInQuery , entity , true);
            
            ECB.SetComponentEnabled<CanRangeAttackTag>(entityIndexInQuery , entity , false);
        }
    }
}