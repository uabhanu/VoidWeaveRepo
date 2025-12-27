namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { state.Dependency = new DamageJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(state.Dependency); }
    }

    [BurstCompile]
    [WithAll(typeof(DamageEventComponent))]
    [WithNone(typeof(DeathTag))]
    public partial struct DamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in DamageEventComponent damageEventComponent , Entity entity , ref HealthComponent healthComponent , [EntityIndexInQuery] int sortKey)
        {
            healthComponent.Health -= damageEventComponent.Damage;
            
            // If Health <= 0, we Add DeathTag
            for(int i = 0 ; i < math.select(0 , 1 , healthComponent.Health <= 0) ; i++) { ECB.AddComponent<DeathTag>(sortKey , entity); }
            
            ECB.RemoveComponent<DamageEventComponent>(sortKey , entity);
        }
    }
}