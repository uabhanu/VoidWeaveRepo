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

            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<LootSpawnedFirstTimeComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var lootQuery = SystemAPI.QueryBuilder().WithAll<LootTutorialActiveTag>().Build();

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            
            int lootSpawnedFirstTimeValue = SystemAPI.GetSingleton<LootSpawnedFirstTimeComponent>().Value;
            bool shouldUpdate = !lootQuery.IsEmpty;
            SystemAPI.SetSingleton(new LootSpawnedFirstTimeComponent { Value = math.select(lootSpawnedFirstTimeValue , doAction , shouldUpdate) });

            new SpawnLootJob { DoAction = doAction , ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , LootSpawnedFirstTimeValue = lootSpawnedFirstTimeValue , NoAction = noAction }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(DeathTag))]
    public partial struct SpawnLootJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int LootSpawnedFirstTimeValue;
        public int NoAction;

        private void Execute([EntityIndexInQuery] int entityIndexInQuery , in LocalTransform localToWorld , in LootAmountComponent lootAmountComponent , in LootEntityComponent lootEntityComponent)
        {
            Entity newLoot = ECB.Instantiate(entityIndexInQuery , lootEntityComponent.Entity);

            ECB.SetComponent(entityIndexInQuery , newLoot , LocalTransform.FromPosition(localToWorld.Position));
            ECB.SetComponent(entityIndexInQuery , newLoot , new LootAmountComponent { Value = lootAmountComponent.Value });

            int isFirstLoot = math.select(NoAction , DoAction , LootSpawnedFirstTimeValue == NoAction);

            for(int i = NoAction ; i < isFirstLoot ; i++)
            {
                ECB.AddComponent(entityIndexInQuery , newLoot , new LootTutorialActiveTag());
                ECB.AddComponent(entityIndexInQuery , ECB.CreateEntity(entityIndexInQuery) , new LootTutorialPauseTag());
            }
        }
    }
}