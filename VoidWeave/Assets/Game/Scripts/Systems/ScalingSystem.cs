namespace Game.Scripts.Systems
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
            systemState.RequireForUpdate<ScalingBaseComponent>();
            systemState.RequireForUpdate<ScalingLevelOffsetComponent>();
            systemState.RequireForUpdate<ScalingMinLevelComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float scalingBase = SystemAPI.GetSingleton<ScalingBaseComponent>().Value;
            int scalingLevelOffset = SystemAPI.GetSingleton<ScalingLevelOffsetComponent>().Value;
            int scalingMinLevel = SystemAPI.GetSingleton<ScalingMinLevelComponent>().Value;

            systemState.Dependency = new ScalingJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Value , DamageMultiplier = SystemAPI.GetSingleton<DamageMultiplierComponent>().Value , EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , HealthMultiplier = SystemAPI.GetSingleton<HealthMultiplierComponent>().Value , LootMultiplier = SystemAPI.GetSingleton<LootMultiplierComponent>().Value , ScalingBase = scalingBase , ScalingLevelOffset = scalingLevelOffset , ScalingMinLevel = scalingMinLevel }.ScheduleParallel(systemState.Dependency);
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
        public float ScalingBase;
        public int ScalingLevelOffset;
        public int ScalingMinLevel;

        private void Execute(ref CurrentHealthComponent currentHealthComponent , ref DamageComponent damageComponent , in EnemyJustSpawnedTag enemyJustSpawnedTag , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref LootAmountComponent lootAmountComponent , ref MaxHealthComponent maxHealthComponent)
        {
            float levelMultiplier = math.max(ScalingMinLevel , CurrentLevel - ScalingLevelOffset);

            damageComponent.Value = (int)math.ceil(damageComponent.Value * (ScalingBase + levelMultiplier * DamageMultiplier));

            var newHealth = (int)math.ceil(maxHealthComponent.Value * (ScalingBase + levelMultiplier * HealthMultiplier));
            maxHealthComponent.Value = newHealth;
            currentHealthComponent.Value = newHealth;

            lootAmountComponent.Value = (int)(lootAmountComponent.Value * (ScalingBase + levelMultiplier * LootMultiplier));

            EntityCommandBufferParallelWriter.RemoveComponent<EnemyJustSpawnedTag>(entityIndexInQuery , entity);
        }
    }
}