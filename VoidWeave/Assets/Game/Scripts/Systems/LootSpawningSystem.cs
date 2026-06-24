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
        private EntityQuery _lootQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _lootQuery = SystemAPI.QueryBuilder().WithAll<LootTutorialActiveTag>().Build();

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<LootSpawnedFirstTimeComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            SystemAPI.SetSingleton(new LootSpawnedFirstTimeComponent { Value = math.select(SystemAPI.GetSingleton<LootSpawnedFirstTimeComponent>().Value , 1 , !_lootQuery.IsEmpty) });
            new SpawnLootJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , LootSpawnedFirstTimeValue = SystemAPI.GetSingleton<LootSpawnedFirstTimeComponent>().Value }.ScheduleParallel();
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

            // =========================================================================================
            // INTENTIONAL ARCHITECTURE EXCEPTION:
            // Do NOT refactor the AddComponent calls below to SetComponentEnabled. 
            // While structural changes are normally avoided during gameplay, Unity DOTS allows a 
            // "Free Structural Change" when ECB.AddComponent immediately follows ECB.Instantiate 
            // within the same buffer tick. The entity spawns directly into its final archetype, 
            // causing zero performance lag. Using SetComponentEnabled here without the tags being 
            // pre-baked into the prefabs will cause a Burst Assert crash and an infinite spawn loop.
            // =========================================================================================

            for(int i = 0 ; i < isFirstLoot ; i++)
            {
                ECB.AddComponent(entityIndexInQuery , newLoot , new LootTutorialActiveTag());
                ECB.AddComponent(entityIndexInQuery , ECB.CreateEntity(entityIndexInQuery) , new LootTutorialPauseTag());
            }
        }
    }
}