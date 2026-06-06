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
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;

            Entity lootSpawnedFirstTimeEntity = SystemAPI.GetSingletonEntity<LootSpawnedFirstTimeComponent>();
            bool lootSpawnedFirstTimeValue = SystemAPI.GetSingleton<LootSpawnedFirstTimeComponent>().Value;

            new SpawnLootJob { DoAction = doAction , ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , LootSpawnedFirstTimeEntity = lootSpawnedFirstTimeEntity , LootSpawnedFirstTimeValue = lootSpawnedFirstTimeValue , NoAction = noAction }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(DeathTag))]
    public partial struct SpawnLootJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity LootSpawnedFirstTimeEntity;
        public bool LootSpawnedFirstTimeValue;
        public int NoAction;

        private void Execute([EntityIndexInQuery] int entityIndexInQuery , in LocalTransform localToWorld , in LootAmountComponent lootAmountComponent , in LootEntityComponent lootEntityComponent)
        {
            Entity newLoot = ECB.Instantiate(entityIndexInQuery , lootEntityComponent.Entity);

            ECB.SetComponent(entityIndexInQuery , newLoot , LocalTransform.FromPosition(localToWorld.Position));
            ECB.SetComponent(entityIndexInQuery , newLoot , new LootAmountComponent { Value = lootAmountComponent.Value });

            int isFirstLoot = math.select(NoAction , DoAction , LootSpawnedFirstTimeValue == false);

            for(int i = NoAction ; i < isFirstLoot ; i++)
            {
                ECB.AddComponent(entityIndexInQuery , newLoot , new LootTutorialActiveTag());
                ECB.AddComponent(entityIndexInQuery , ECB.CreateEntity(entityIndexInQuery) , new LootTutorialPauseTag());
                
                ECB.SetComponent(entityIndexInQuery , LootSpawnedFirstTimeEntity , new LootSpawnedFirstTimeComponent { Value = true });
            }
        }
    }
}