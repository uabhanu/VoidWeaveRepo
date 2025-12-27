namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MeleeAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // 1. ENABLE COOLDOWN (Tag -> Cooldown)
            JobHandle enableHandle = new MeleeAttackEnableJob { ECB = ecb }.ScheduleParallel(state.Dependency);

            // 2. DISABLE COOLDOWN (Tick -> Remove)
            state.Dependency = new MeleeAttackDisableJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = ecb }.ScheduleParallel(enableHandle);
        }
    }

    [BurstCompile]
    [WithAll(typeof(MeleeAttackEventTag))]
    public partial struct MeleeAttackEnableJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in MeleeAttackRateComponent meleeAttackRateComponent)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = meleeAttackRateComponent.MeleeAttackRate });
            ECB.RemoveComponent<MeleeAttackEventTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    public partial struct MeleeAttackDisableJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        
        private void Execute(ref CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in MeleeAttackRateComponent meleeAttackRateComponent)
        {
            cooldownComponent.Timer -= DeltaTime;
            
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0f) ; i++) { ECB.RemoveComponent<CooldownComponent>(entityIndexInQuery , entity); }
        }
    }
}