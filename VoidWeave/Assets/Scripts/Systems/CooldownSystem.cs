namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { state.Dependency = new CooldownJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(state.Dependency); }
    }

    [BurstCompile]
    public partial struct CooldownJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(ref CooldownComponent cooldownComponent , Entity entity , [EntityIndexInQuery] int sortKey)
        {
            cooldownComponent.Timer -= DeltaTime;

            // If Timer <= 0, Remove Component
            for(int i = 0 ; i < math.select(0 , 1 , cooldownComponent.Timer <= 0f) ; i++) { ECB.RemoveComponent<CooldownComponent>(sortKey , entity); }
        }
    }
}