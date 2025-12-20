namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ProjectileLifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ProjectileTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new ProjectileLifetimeJob { DeltaTime = SystemAPI.Time.DeltaTime , EntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(); }
    }

    [BurstCompile]
    public partial struct ProjectileLifetimeJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        
        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , ref ProjectileLifetimeComponent projectileLifetimeComponent)
        {
            projectileLifetimeComponent.Timer -= DeltaTime;

            for(int i = 0 ; i < math.select(0 , 1 , projectileLifetimeComponent.Timer <= 0f) ; i++) { EntityCommandBuffer.DestroyEntity(entityInQueryIndex , entity); }
        }
    }
}