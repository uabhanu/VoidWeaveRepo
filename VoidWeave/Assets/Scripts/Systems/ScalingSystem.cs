namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EnemySpawningSystem))]
    public partial struct ScalingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<DamageMultiplierComponent>();
            systemState.RequireForUpdate<EnemyJustSpawnedTag>();
            systemState.RequireForUpdate<HealthMultiplierComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LootMultiplierComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency = new ScalingJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Level , DamageMultiplier = SystemAPI.GetSingleton<DamageMultiplierComponent>().DamageMultiplier , EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , HealthMultiplier = SystemAPI.GetSingleton<HealthMultiplierComponent>().HealthMultiplier , LootMultiplier = SystemAPI.GetSingleton<LootMultiplierComponent>().LootMultiplier , }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct ScalingJob : IJobEntity
    {
        public int CurrentLevel;
        public float DamageMultiplier;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public float HealthMultiplier;
        public float LootMultiplier;

        private void Execute(ref CurrentHealthComponent currentHealthComponent , ref DamageComponent damageComponent , in EnemyJustSpawnedTag enemyJustSpawnedTag , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref LootAmountComponent lootAmountComponent , ref MaxHealthComponent maxHealthComponent)
        {
            float levelMultiplier = math.max(0 , CurrentLevel - 3);

            damageComponent.Damage = (int)math.ceil(damageComponent.Damage * (1f + levelMultiplier * DamageMultiplier));

            int newHealth = (int)math.ceil(maxHealthComponent.MaxHealth * (1f + levelMultiplier * HealthMultiplier));
            maxHealthComponent.MaxHealth = newHealth;
            currentHealthComponent.CurrentHealth = newHealth;

            lootAmountComponent.Amount = (int)(lootAmountComponent.Amount * (1f + levelMultiplier * LootMultiplier));

            EntityCommandBufferParallelWriter.RemoveComponent<EnemyJustSpawnedTag>(entityIndexInQuery , entity);
        }
    }
}