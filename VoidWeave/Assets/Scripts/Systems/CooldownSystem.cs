namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Consumes ProjectileSpawnedTag -> Adds CooldownComponent
            JobHandle applyHandle = new AddCooldownJob { ECB = ecb }.ScheduleParallel(state.Dependency);

            // Decrements Timer -> Removes CooldownComponent
            state.Dependency = new RemoveCooldownJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = ecb }.ScheduleParallel(applyHandle);
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(ProjectileSpawnedTag))]
    public partial struct AddCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey , in FireRateComponent fireRateComponent)
        {
            ECB.AddComponent(sortKey , entity , new CooldownComponent { Timer = fireRateComponent.FireRate });
            ECB.RemoveComponent<ProjectileSpawnedTag>(sortKey , entity);
        }
    }
    
    [BurstCompile]
    public partial struct RemoveCooldownJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey , ref CooldownComponent cooldownComponent)
        {
            cooldownComponent.Timer -= DeltaTime;
            
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0f) ; i++) { ECB.RemoveComponent<CooldownComponent>(sortKey , entity); }
        }
    }
}