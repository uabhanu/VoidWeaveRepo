namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct RangedAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new RangedAttackJob { ECB = ecb }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileSpawnedEventTag))]
    public partial struct RangedAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in FireRateComponent fireRateComponent)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = fireRateComponent.FireRate });
            ECB.RemoveComponent<ProjectileSpawnedEventTag>(entityIndexInQuery , entity);
        }
    }
}