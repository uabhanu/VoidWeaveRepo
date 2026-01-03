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
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            
            JobHandle activeJobHandle = new CooldownActiveJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = ecb }.ScheduleParallel(systemState.Dependency);
            
            systemState.Dependency = new CooldownStartJob { ECB = ecb }.ScheduleParallel(activeJobHandle);
        }
    }

    [BurstCompile]
    public partial struct CooldownActiveJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(ref CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery)
        {
            cooldownComponent.Timer -= DeltaTime;
            
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0f) ; i++) { ECB.RemoveComponent<CooldownComponent>(entityIndexInQuery , entity); }
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileSpawnedEventTag))]
    public partial struct CooldownStartJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in FireRateComponent fireRateComponent)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = fireRateComponent.FireRate });
            ECB.RemoveComponent<ProjectileSpawnedEventTag>(entityIndexInQuery , entity);
        }
    }
}