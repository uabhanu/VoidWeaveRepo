namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(LifetimeSystem))]
    public partial struct EntityPulseSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { new PulseJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(); }
    }

    [BurstCompile]
    [WithAll(typeof(LootTag))]
    [WithNone(typeof(PulseTag))]
    public partial struct PulseJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , in LifetimeComponent lifetimeComponent , in TimeBeforeEntityPulseComponent timeBeforePulse)
        {
            for(var i = 0 ; i < math.select(0 , 1 , lifetimeComponent.Value <= timeBeforePulse.Value) ; i++) ECB.SetComponentEnabled<PulseTag>(entityInQueryIndex , entity , true);
        }
    }
}