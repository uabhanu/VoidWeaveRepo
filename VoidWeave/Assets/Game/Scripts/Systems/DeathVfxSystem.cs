namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(DeathSystem))]
    public partial struct DeathVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }
        
        public void OnUpdate(ref SystemState systemState) { new DeathVfxJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(); }
    }

    [BurstCompile]
    [WithAll(typeof(DeathTag))]
    public partial struct DeathVfxJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in DeathVfxComponent deathVfxComponent , [EntityIndexInQuery] int entityIndexInQuery , in LocalTransform localTransform)
        {
            Entity vfxEntity = ECB.Instantiate(entityIndexInQuery , deathVfxComponent.Value);

            ECB.SetComponentEnabled<VfxUpdateTag>(entityIndexInQuery , vfxEntity , true);
            ECB.SetComponent(entityIndexInQuery , vfxEntity , localTransform);
        }
    }
}