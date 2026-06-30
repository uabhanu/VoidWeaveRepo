namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(AdvanceLevelSystem))]
    public partial struct EvaluateLevelWinConditionSystem : ISystem
    {
        private EntityQuery _lootQuery;
        
        public void OnCreate(ref SystemState systemState)
        {
            _lootQuery = SystemAPI.QueryBuilder().WithAll<LootPickupTag>().Build();
            
            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
            systemState.RequireForUpdate<IsTestingComponent>();
        }
        
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