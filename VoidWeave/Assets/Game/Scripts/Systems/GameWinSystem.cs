namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(LevelProgressionSystem))]
    public partial struct GameWinSystem : ISystem
    {
        private EntityQuery _lootQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _lootQuery = SystemAPI.QueryBuilder().WithAll<LootPickupTag>().Build();

            systemState.RequireForUpdate<EnemiesKilledComponent>();
            systemState.RequireForUpdate<EnemiesToKillComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int enemiesKilled = SystemAPI.GetSingleton<EnemiesKilledComponent>().Value;
            int enemiesToKill = SystemAPI.GetSingleton<EnemiesToKillComponent>().Value;

            bool levelObjectiveComplete = enemiesKilled >= enemiesToKill;
            bool noLootRemaining = _lootQuery.IsEmpty;

            bool levelWon = levelObjectiveComplete && noLootRemaining;

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithAll<GameWonTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess()) { SystemAPI.SetComponentEnabled<GameWonTag>(entity , levelWon); }
        }
    }
}