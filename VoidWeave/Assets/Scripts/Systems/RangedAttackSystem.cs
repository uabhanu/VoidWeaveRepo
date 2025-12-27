namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct RangedAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Consumes ProjectileSpawnedEventTag -> Adds CooldownComponent
            JobHandle cooldownEnableJob = new CooldownEnableJob { ECB = ecb }.ScheduleParallel(state.Dependency);

            // Decrements Timer -> Removes CooldownComponent
            state.Dependency = new CooldownDisableJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = ecb }.ScheduleParallel(cooldownEnableJob);
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(ProjectileSpawnedEventTag))]
    public partial struct CooldownEnableJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in FireRateComponent fireRateComponent)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = fireRateComponent.FireRate });
            ECB.RemoveComponent<ProjectileSpawnedEventTag>(entityIndexInQuery , entity);
        }
    }
    
    [BurstCompile]
    public partial struct CooldownDisableJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(ref CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            cooldownComponent.Timer -= DeltaTime;
            
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0f) ; i++) { ECB.RemoveComponent<CooldownComponent>(entityIndexInQuery , entity); }
        }
    }
}