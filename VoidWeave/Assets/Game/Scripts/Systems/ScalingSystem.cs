namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(EnemySpawningSystem))]
    public partial struct ScalingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<DamageMultiplierComponent>();
            systemState.RequireForUpdate<EliteStatMultiplierComponent>();
            systemState.RequireForUpdate<EnemyJustSpawnedTag>();
            systemState.RequireForUpdate<HealthMultiplierComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LootMultiplierComponent>();
            systemState.RequireForUpdate<MaxCampaignLevelComponent>();
            systemState.RequireForUpdate<NormalStatMultiplierComponent>();
            systemState.RequireForUpdate<ScalingBaseComponent>();
            systemState.RequireForUpdate<ScalingLevelOffsetComponent>();
            systemState.RequireForUpdate<ScalingMinLevelComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float eliteStatMultiplier = SystemAPI.GetSingleton<EliteStatMultiplierComponent>().Value;
            int maxCampaignLevel = SystemAPI.GetSingleton<MaxCampaignLevelComponent>().Value;
            float normalStatMultiplier = SystemAPI.GetSingleton<NormalStatMultiplierComponent>().Value;
            float scalingBase = SystemAPI.GetSingleton<ScalingBaseComponent>().Value;
            int scalingLevelOffset = SystemAPI.GetSingleton<ScalingLevelOffsetComponent>().Value;
            int scalingMinLevel = SystemAPI.GetSingleton<ScalingMinLevelComponent>().Value;

            systemState.Dependency = new ScalingJob
            {
                CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Value ,
                DamageMultiplier = SystemAPI.GetSingleton<DamageMultiplierComponent>().Value ,
                EliteStatMultiplier = eliteStatMultiplier ,
                EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() ,
                HealthMultiplier = SystemAPI.GetSingleton<HealthMultiplierComponent>().Value ,
                LootMultiplier = SystemAPI.GetSingleton<LootMultiplierComponent>().Value ,
                MaxCampaignLevel = maxCampaignLevel ,
                NormalStatMultiplier = normalStatMultiplier ,
                ScalingBase = scalingBase ,
                ScalingLevelOffset = scalingLevelOffset ,
                ScalingMinLevel = scalingMinLevel
            }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct ScalingJob : IJobEntity
    {
        public int CurrentLevel;
        public float DamageMultiplier;
        public float EliteStatMultiplier;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public float HealthMultiplier;
        public float LootMultiplier;
        public int MaxCampaignLevel;
        public float NormalStatMultiplier;
        public float ScalingBase;
        public int ScalingLevelOffset;
        public int ScalingMinLevel;

        private void Execute(ref CurrentHealthComponent currentHealthComponent , ref DamageComponent damageComponent , in EnemyJustSpawnedTag enemyJustSpawnedTag , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref LootAmountComponent lootAmountComponent , ref MaxHealthComponent maxHealthComponent)
        {
            int cappedLevel = math.min(CurrentLevel , MaxCampaignLevel);
            float eliteMultiplier = math.select(NormalStatMultiplier , EliteStatMultiplier , CurrentLevel == MaxCampaignLevel);
            float levelMultiplier = math.max(ScalingMinLevel , CurrentLevel - ScalingLevelOffset);

            damageComponent.Value = (int)math.ceil(damageComponent.Value * (ScalingBase + levelMultiplier * DamageMultiplier));

            var newHealth = (int)math.ceil(maxHealthComponent.Value * (ScalingBase + levelMultiplier * HealthMultiplier) * eliteMultiplier);
            maxHealthComponent.Value = newHealth;
            currentHealthComponent.Value = newHealth;

            lootAmountComponent.Value = (int)(lootAmountComponent.Value * (ScalingBase + levelMultiplier * LootMultiplier) * eliteMultiplier);

            EntityCommandBufferParallelWriter.RemoveComponent<EnemyJustSpawnedTag>(entityIndexInQuery , entity);
        }
    }
}