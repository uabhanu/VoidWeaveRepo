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

            systemState.RequireForUpdate<EliteStatMultiplierComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<LastLevelComponent>();
            systemState.RequireForUpdate<NormalStatMultiplierComponent>();
            systemState.RequireForUpdate<ScalingBaseComponent>();
            systemState.RequireForUpdate<ScalingLevelOffsetComponent>();
            systemState.RequireForUpdate<ScalingMinLevelComponent>();

            systemState.RequireForUpdate<ScaleStatsTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency = new DisableScaleTagJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new EnemyScalingJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Value , EliteStatMultiplier = SystemAPI.GetSingleton<EliteStatMultiplierComponent>().Value , LastLevel = SystemAPI.GetSingleton<LastLevelComponent>().Value , NormalStatMultiplier = SystemAPI.GetSingleton<NormalStatMultiplierComponent>().Value , ScalingBase = SystemAPI.GetSingleton<ScalingBaseComponent>().Value , ScalingLevelOffset = SystemAPI.GetSingleton<ScalingLevelOffsetComponent>().Value , ScalingMinLevel = SystemAPI.GetSingleton<ScalingMinLevelComponent>().Value }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new PlayerScalingJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Value , EliteStatMultiplier = SystemAPI.GetSingleton<EliteStatMultiplierComponent>().Value , LastLevel = SystemAPI.GetSingleton<LastLevelComponent>().Value , NormalStatMultiplier = SystemAPI.GetSingleton<NormalStatMultiplierComponent>().Value , ScalingBase = SystemAPI.GetSingleton<ScalingBaseComponent>().Value , ScalingLevelOffset = SystemAPI.GetSingleton<ScalingLevelOffsetComponent>().Value , ScalingMinLevel = SystemAPI.GetSingleton<ScalingMinLevelComponent>().Value }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct DisableScaleTagJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in ScaleStatsTag scaleStatsTag) { ECB.SetComponentEnabled<ScaleStatsTag>(entityIndexInQuery , entity , false); }
    }

    [BurstCompile]
    public partial struct EnemyScalingJob : IJobEntity
    {
        public int CurrentLevel;
        public float EliteStatMultiplier;
        public int LastLevel;
        public float NormalStatMultiplier;
        public float ScalingBase;
        public int ScalingLevelOffset;
        public int ScalingMinLevel;

        private void Execute(ref CurrentHealthComponent currentHealthComponent , ref DamageComponent damageComponent , in EnemyTag enemyTag , ref LootAmountComponent lootAmountComponent , ref MaxHealthComponent maxHealthComponent , in ScaleStatsTag scaleStatsTag)
        {
            int cappedLevel = math.min(CurrentLevel , LastLevel);
            float levelMultiplier = math.max(ScalingMinLevel , cappedLevel - ScalingLevelOffset);
            float eliteMultiplier = math.select(NormalStatMultiplier , EliteStatMultiplier , cappedLevel >= LastLevel);
            float totalMultiplier = ScalingBase + levelMultiplier * eliteMultiplier;

            damageComponent.Value = (int)math.ceil(damageComponent.Value * totalMultiplier);

            var newHealth = (int)math.ceil(maxHealthComponent.Value * totalMultiplier);
            maxHealthComponent.Value = newHealth;
            currentHealthComponent.Value = newHealth;

            lootAmountComponent.Value = (int)(lootAmountComponent.Value * totalMultiplier);
        }
    }

    [BurstCompile]
    public partial struct PlayerScalingJob : IJobEntity
    {
        public int CurrentLevel;
        public float EliteStatMultiplier;
        public int LastLevel;
        public float NormalStatMultiplier;
        public float ScalingBase;
        public int ScalingLevelOffset;
        public int ScalingMinLevel;

        private void Execute(ref CurrentHealthComponent currentHealthComponent , ref MaxHealthComponent maxHealthComponent , in PlayerTag playerTag , in ScaleStatsTag scaleStatsTag)
        {
            int cappedLevel = math.min(CurrentLevel , LastLevel);
            float levelMultiplier = math.max(ScalingMinLevel , cappedLevel - ScalingLevelOffset);
            float eliteMultiplier = math.select(NormalStatMultiplier , EliteStatMultiplier , cappedLevel >= LastLevel);
            float totalMultiplier = ScalingBase + levelMultiplier * eliteMultiplier;

            var newHealth = (int)math.ceil(maxHealthComponent.Value * totalMultiplier);
            maxHealthComponent.Value = newHealth;
            currentHealthComponent.Value = newHealth;
        }
    }
}