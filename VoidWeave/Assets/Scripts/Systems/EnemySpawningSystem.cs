namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(LevelProgressionSystem))]
    [UpdateAfter(typeof(TimerSystem))]
    [UpdateAfter(typeof(WaveStateSystem))]
    public partial struct EnemySpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag>().Build());
            systemState.RequireForUpdate<LevelComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerTag>();

            Entity lineEnemyEntity = SystemAPI.GetComponent<LineEnemyEntityComponent>(spawnerEntity).Entity;
            Entity squareEnemyEntity = SystemAPI.GetComponent<SquareEnemyEntityComponent>(spawnerEntity).Entity;
            Entity triangleEnemyEntity = SystemAPI.GetComponent<TriangleEnemyEntityComponent>(spawnerEntity).Entity;

            float lineEnemyBaseDamage = SystemAPI.GetComponent<DamageComponent>(lineEnemyEntity).Damage;
            float lineEnemyBaseHealth = SystemAPI.GetComponent<MaxHealthComponent>(lineEnemyEntity).MaxHealth;
            int lineEnemyBaseLoot = SystemAPI.GetComponent<LootAmountComponent>(lineEnemyEntity).Amount;

            float squareEnemyBaseDamage = SystemAPI.GetComponent<DamageComponent>(squareEnemyEntity).Damage;
            float squareEnemyBaseHealth = SystemAPI.GetComponent<MaxHealthComponent>(squareEnemyEntity).MaxHealth;
            int squareEnemyBaseLoot = SystemAPI.GetComponent<LootAmountComponent>(squareEnemyEntity).Amount;

            float triangleEnemyBaseDamage = SystemAPI.GetComponent<DamageComponent>(triangleEnemyEntity).Damage;
            float triangleEnemyBaseHealth = SystemAPI.GetComponent<MaxHealthComponent>(triangleEnemyEntity).MaxHealth;
            int triangleEnemyBaseLoot = SystemAPI.GetComponent<LootAmountComponent>(triangleEnemyEntity).Amount;

            systemState.Dependency = new EnemySpawnJob
            {
                CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Level ,
                DamageMultiplier = SystemAPI.GetComponent<DamageMultiplierComponent>(spawnerEntity).DamageMultiplier ,
                EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() ,
                HealthMultiplier = SystemAPI.GetComponent<HealthMultiplierComponent>(spawnerEntity).HealthMultiplier ,
                LineBaseDamage = lineEnemyBaseDamage ,
                LineBaseHealth = lineEnemyBaseHealth ,
                LineBaseLoot = lineEnemyBaseLoot ,
                LootMultiplier = SystemAPI.GetComponent<LootMultiplierComponent>(spawnerEntity).LootMultiplier ,
                PlayerCount = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build().CalculateEntityCount() ,
                SquareBaseDamage = squareEnemyBaseDamage ,
                SquareBaseHealth = squareEnemyBaseHealth ,
                SquareBaseLoot = squareEnemyBaseLoot ,
                TriangleBaseDamage = triangleEnemyBaseDamage ,
                TriangleBaseHealth = triangleEnemyBaseHealth ,
                TriangleBaseLoot = triangleEnemyBaseLoot ,
            }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public int CurrentLevel;
        public float DamageMultiplier;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public float HealthMultiplier;
        public float LineBaseDamage;
        public float LineBaseHealth;
        public int LineBaseLoot;
        public float LootMultiplier;
        public int PlayerCount;
        public float SquareBaseDamage;
        public float SquareBaseHealth;
        public int SquareBaseLoot;
        public float TriangleBaseDamage;
        public float TriangleBaseHealth;
        public int TriangleBaseLoot;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , in LineEnemyEntityComponent lineEnemyEntityComponent , in LocalTransform localTransform , ref RandomComponent randomComponent , in SquareEnemyEntityComponent squareEnemyEntityComponent , ref TimerComponent timerComponent , in TriangleEnemyEntityComponent triangleEnemyEntityComponent , in UnlockedEnemiesComponent unlockedEnemiesComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            bool canSpawn = timerComponent.Timer <= 0f && PlayerCount > 0 && waveStateComponent.State == 1 && waveStockComponent.Stock > 0;

            for(int i = 0 ; i < math.select(0 , 1 , canSpawn) ; i++)
            {
                int enemyTypeIndex = randomComponent.Random.NextInt(0 , 3);

                bool isUnlocked = (unlockedEnemiesComponent.UnlockedEnemiesBitmask & (1u << enemyTypeIndex)) != 0;

                enemyTypeIndex = math.select(0 , enemyTypeIndex , isUnlocked);
                
                Entity enemyEntityToSpawn = enemyTypeIndex == 1 ? lineEnemyEntityComponent.Entity : enemyTypeIndex == 2 ? squareEnemyEntityComponent.Entity : triangleEnemyEntityComponent.Entity;
                Entity newEnemyEntity = EntityCommandBufferParallelWriter.Instantiate(entityInQueryIndex , enemyEntityToSpawn);

                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , LocalTransform.FromPosition(localTransform.Position + new float3(randomComponent.Random.NextFloat2Direction() * enemySpawnRadiusComponent.Radius , 0f)));
                
                // Logic: Start scaling ONLY after Level 3 (Triangle=1, Line=2, Square=3). Level 4 is the first boost.
                float levelMultiplier = math.max(0 , CurrentLevel - 3);

                float selectedEnemyBaseDamage = math.select(TriangleBaseDamage , math.select(LineBaseDamage , SquareBaseDamage , enemyTypeIndex == 2) , enemyTypeIndex >= 1);
                int newDamage = (int)math.ceil(selectedEnemyBaseDamage * (1f + levelMultiplier * DamageMultiplier));
                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , new DamageComponent { Damage = newDamage });
                
                float selectedEnemyBaseHealth = math.select(TriangleBaseHealth , math.select(LineBaseHealth , SquareBaseHealth , enemyTypeIndex == 2) , enemyTypeIndex >= 1);
                int newHealth = (int)math.ceil(selectedEnemyBaseHealth * (1f + levelMultiplier * HealthMultiplier));
                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , new CurrentHealthComponent { CurrentHealth = newHealth });
                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , new MaxHealthComponent { MaxHealth = newHealth });

                int selectedEnemyBaseLoot = math.select(TriangleBaseLoot , math.select(LineBaseLoot , SquareBaseLoot , enemyTypeIndex == 2) , enemyTypeIndex >= 1);
                int newLoot = (int)(selectedEnemyBaseLoot * (1f + (levelMultiplier * LootMultiplier)));

                EntityCommandBufferParallelWriter.SetComponent(entityInQueryIndex , newEnemyEntity , new LootAmountComponent { Amount = newLoot });

                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == 0) ; k++) { EntityCommandBufferParallelWriter.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemyEntity); }
                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == 1) ; k++) { EntityCommandBufferParallelWriter.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemyEntity); }
                for(int k = 0 ; k < math.select(0 , 1 , enemyTypeIndex == 2) ; k++) { EntityCommandBufferParallelWriter.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemyEntity); }
            }

            timerComponent.Timer = math.select(timerComponent.Timer , enemySpawnRateComponent.Rate , canSpawn);
            waveStockComponent.Stock -= math.select(0 , 1 , canSpawn);
        }
    }
}