namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemySpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EnemySpawnerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { state.Dependency = new EnemySpawnJob { DeltaTime = SystemAPI.Time.DeltaTime , EntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , PlayerCount = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build().CalculateEntityCount() }.ScheduleParallel(state.Dependency); }
    }

    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public int PlayerCount;

        private void Execute(in BulletEntityComponent bulletEntityComponent , in DamageComponent damageComponent , [EntityIndexInQuery] int entityInQueryIndex , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , ref EnemySpawnTimerComponent enemySpawnTimerComponent , in FireRateComponent fireRateComponent , in LineEnemyEntityComponent lineEnemyEntityComponent , in LocalTransform localTransform , in ProjectileCountComponent projectileCountComponent , ref RandomComponent randomComponent , in RangeComponent rangeComponent , in SpreadComponent spreadComponent , in SquareEnemyEntityComponent squareEnemyEntityComponent , in TriangleEnemyEntityComponent triangleEnemyEntityComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            enemySpawnTimerComponent.Timer -= DeltaTime;

            for(int i = 0 ; i < math.select(0 , 1 , enemySpawnTimerComponent.Timer <= 0f && PlayerCount > 0 && waveStateComponent.State == 1 && waveStockComponent.Stock > 0) ; i++)
            {
                // Necessary to sync Prefab choice with Tag choice
                int selection = randomComponent.Random.NextInt(0 , 3);

                // Necessary to capture the ID so we can move it
                // Inline Ternary: 0=Basic, 1=Fast, 2=Tank
                Entity newEnemy = EntityCommandBuffer.Instantiate(entityInQueryIndex , selection == 1 ? lineEnemyEntityComponent.Entity : (selection == 2 ? squareEnemyEntityComponent.Entity : triangleEnemyEntityComponent.Entity));

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , LocalTransform.FromPosition(localTransform.Position + new float3(randomComponent.Random.NextFloat2Direction() * enemySpawnRadiusComponent.Radius , 0f)));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , new MovementInputComponent { Input = float2.zero });

                // Triangle Logic (Selection 0)
                for(int k = 0 ; k < math.select(0 , 1 , selection == 0) ; k++) { EntityCommandBuffer.AddComponent<TriangleEnemyTag>(entityInQueryIndex , newEnemy); }
                
                // Line Logic (Selection 1)
                for(int k = 0 ; k < math.select(0 , 1 , selection == 1) ; k++) { EntityCommandBuffer.AddComponent<LineEnemyTag>(entityInQueryIndex , newEnemy); }

                // Square Logic (Selection 2) - Now correctly receives Turret Components
                for(int k = 0 ; k < math.select(0 , 1 , selection == 2) ; k++)
                {
                    EntityCommandBuffer.AddComponent<SquareEnemyTag>(entityInQueryIndex , newEnemy);

                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , bulletEntityComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , damageComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , fireRateComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , projectileCountComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , rangeComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , spreadComponent);
                }
            }

            waveStockComponent.Stock -= math.select(0 , 1 , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0);
            enemySpawnTimerComponent.Timer = math.select(enemySpawnTimerComponent.Timer , enemySpawnRateComponent.Rate , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0);
        }
    }
}