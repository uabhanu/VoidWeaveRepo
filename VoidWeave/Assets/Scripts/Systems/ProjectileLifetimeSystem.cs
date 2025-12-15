using Components;

namespace Systems
{
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
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            BeginSimulationEntityCommandBufferSystem.Singleton entityCommandBufferSystemSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer.ParallelWriter entityCommandBufferParallel = entityCommandBufferSystemSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ProjectileLifetimeJob { DeltaTime = deltaTime , EntityCommandBuffer = entityCommandBufferParallel }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ProjectileLifetimeJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , Entity entity , ref ProjectileLifetimeComponent projectileLifetimeComponent)
        {
            projectileLifetimeComponent.Timer -= DeltaTime;
            
            float isExpired = math.step(projectileLifetimeComponent.Timer , 0f);
            
            int destroyCount = (int)isExpired;

            for(int i = 0 ; i < destroyCount ; i++) { EntityCommandBuffer.DestroyEntity(entityInQueryIndex , entity); }
        }
    }
}