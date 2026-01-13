namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EnemySpawningSystem))]
    public partial struct EnemyUpgradeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<EnemyJustSpawnedTag>();
            systemState.RequireForUpdate<EnemySpawnerTag>();
            systemState.RequireForUpdate<LevelComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerTag>();

            systemState.Dependency = new EnemyUpgradeJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Level , DamageMultiplier = SystemAPI.GetComponent<DamageMultiplierComponent>(spawnerEntity).DamageMultiplier , EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , HealthMultiplier = SystemAPI.GetComponent<HealthMultiplierComponent>(spawnerEntity).HealthMultiplier , LootMultiplier = SystemAPI.GetComponent<LootMultiplierComponent>(spawnerEntity).LootMultiplier , }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct EnemyUpgradeJob : IJobEntity
    {
        public int CurrentLevel;
        public float DamageMultiplier;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public float HealthMultiplier;
        public float LootMultiplier;

        private void Execute(ref CurrentHealthComponent currentHealthComponent , ref DamageComponent damageComponent , in EnemyJustSpawnedTag enemyJustSpawnedTag , Entity entity , [EntityIndexInQuery] int entityInQueryIndex , ref LootAmountComponent lootAmountComponent , ref MaxHealthComponent maxHealthComponent)
        {
            float levelMultiplier = math.max(0 , CurrentLevel - 3);

            damageComponent.Damage = (int)math.ceil(damageComponent.Damage * (1f + levelMultiplier * DamageMultiplier));

            int newHealth = (int)math.ceil(maxHealthComponent.MaxHealth * (1f + levelMultiplier * HealthMultiplier));
            maxHealthComponent.MaxHealth = newHealth;
            currentHealthComponent.CurrentHealth = newHealth;

            lootAmountComponent.Amount = (int)(lootAmountComponent.Amount * (1f + (levelMultiplier * LootMultiplier)));

            EntityCommandBufferParallelWriter.RemoveComponent<EnemyJustSpawnedTag>(entityInQueryIndex , entity);
        }
    }
}