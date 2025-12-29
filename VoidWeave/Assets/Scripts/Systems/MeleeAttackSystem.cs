namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MeleeAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new MeleeAttackJob { ECB = ecb }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(MeleeAttackEventTag))]
    public partial struct MeleeAttackJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in MeleeAttackRateComponent meleeAttackRateComponent)
        {
            ECB.AddComponent(entityIndexInQuery , entity , new CooldownComponent { Timer = meleeAttackRateComponent.MeleeAttackRate });
            ECB.RemoveComponent<MeleeAttackEventTag>(entityIndexInQuery , entity);
        }
    }
}