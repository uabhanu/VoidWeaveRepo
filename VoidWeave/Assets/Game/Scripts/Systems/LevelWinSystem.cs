namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(CampaignProgressionSystem))]
    public partial struct LevelWinSystem : ISystem
    {
        private EntityQuery _lootQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<IsTestingComponent>();
            _lootQuery = SystemAPI.QueryBuilder().WithAll<LootPickupTag>().Build();

            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int enemiesKilled = SystemAPI.GetSingleton<EnemiesKilledComponent>().Value;
            int enemiesToKill = SystemAPI.GetSingleton<EnemiesToKillComponent>().Value;
            bool noLootRemaining = _lootQuery.IsEmpty;
            bool levelObjectiveComplete = enemiesKilled >= enemiesToKill;
            bool levelWon = levelObjectiveComplete && noLootRemaining;

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithAll<LevelWonTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess()) { SystemAPI.SetComponentEnabled<LevelWonTag>(entity , levelWon); }
        }
    }
}