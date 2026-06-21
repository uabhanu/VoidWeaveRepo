namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    [UpdateBefore(typeof(DeathSystem))]
    public partial struct LootSpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<LootSpawnedFirstTimeComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            
            var lootQuery = SystemAPI.QueryBuilder().WithAll<LootTutorialActiveTag>().Build();
            
            int lootSpawnedFirstTimeValue = SystemAPI.GetSingleton<LootSpawnedFirstTimeComponent>().Value;
            bool shouldUpdate = !lootQuery.IsEmpty;
            SystemAPI.SetSingleton(new LootSpawnedFirstTimeComponent { Value = math.select(lootSpawnedFirstTimeValue , 1 , shouldUpdate) });

            new SpawnLootJob { ECB = ecb , LootSpawnedFirstTimeValue = lootSpawnedFirstTimeValue }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(DeathTag))]
    public partial struct SpawnLootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public int LootSpawnedFirstTimeValue;

        private void Execute([EntityIndexInQuery] int entityIndexInQuery , in LocalTransform localToWorld , in LootAmountComponent lootAmountComponent , in LootEntityComponent lootEntityComponent)
        {
            Entity newLoot = ECB.Instantiate(entityIndexInQuery , lootEntityComponent.Entity);

            ECB.SetComponent(entityIndexInQuery , newLoot , LocalTransform.FromPosition(localToWorld.Position));
            ECB.SetComponent(entityIndexInQuery , newLoot , new LootAmountComponent { Value = lootAmountComponent.Value });

            int isFirstLoot = math.select(0 , 1 , LootSpawnedFirstTimeValue == 0);

            for(int i = 0 ; i < isFirstLoot ; i++)
            {
                ECB.AddComponent(entityIndexInQuery , newLoot , new LootTutorialActiveTag());
                ECB.AddComponent(entityIndexInQuery , ECB.CreateEntity(entityIndexInQuery) , new LootTutorialPauseTag());
            }
        }
    }
}