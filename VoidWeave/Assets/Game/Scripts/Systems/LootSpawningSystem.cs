namespace Game.Scripts.Systems
{
    using Game.Scripts.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    [UpdateBefore(typeof(DeathSystem))]
    public partial struct LootSpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new SpawnLootJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(); }
    }

    [BurstCompile]
    [WithAll(typeof(DeathTag))]
    public partial struct SpawnLootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute([EntityIndexInQuery] int entityIndexInQuery , in LocalTransform localToWorld , in LootAmountComponent lootAmountComponent , in LootEntityComponent lootEntityComponent)
        {
            Entity newLoot = ECB.Instantiate(entityIndexInQuery , lootEntityComponent.Entity);

            ECB.SetComponent(entityIndexInQuery , newLoot , LocalTransform.FromPosition(localToWorld.Position));
            ECB.SetComponent(entityIndexInQuery , newLoot , new LootAmountComponent { Value = lootAmountComponent.Value });
        }
    }
}