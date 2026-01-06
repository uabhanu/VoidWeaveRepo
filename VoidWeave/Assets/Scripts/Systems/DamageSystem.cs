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
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new DamageJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    [WithAll(typeof(DamageEventComponent))]
    [WithNone(typeof(DeathTag))]
    public partial struct DamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in DamageEventComponent damageEventComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref CurrentHealthComponent currentHealthComponent)
        {
            currentHealthComponent.CurrentHealth -= damageEventComponent.Damage;
            
            for(int i = 0 ; i < math.select(0 , 1 , currentHealthComponent.CurrentHealth <= 0) ; i++) { ECB.AddComponent<DeathTag>(entityIndexInQuery , entity); }
            
            ECB.RemoveComponent<DamageEventComponent>(entityIndexInQuery , entity);
        }
    }
}