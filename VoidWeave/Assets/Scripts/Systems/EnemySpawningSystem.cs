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

        private void Execute(in BasicEnemyEntityComponent basicEnemyEntityComponent , in BulletEntityComponent bulletEntityComponent , in EnemySpawnRadiusComponent enemySpawnRadiusComponent , in EnemySpawnRateComponent enemySpawnRateComponent , ref EnemySpawnTimerComponent enemySpawnTimerComponent , [EntityIndexInQuery] int entityInQueryIndex , in FastEnemyEntityComponent fastEnemyEntityComponent , in LocalTransform localTransform , ref RandomComponent randomComponent , in SlowEnemyEntityComponent slowEnemyEntityComponent , in TurretDamageComponent turretDamageComponent , in TurretFireRateComponent turretFireRateComponent , in TurretProjectileCountComponent turretProjectileCountComponent , in TurretRangeComponent turretRangeComponent , in TurretSpreadComponent turretSpreadComponent , in WaveStateComponent waveStateComponent , ref WaveStockComponent waveStockComponent)
        {
            enemySpawnTimerComponent.Timer -= DeltaTime;

            for(int i = 0 ; i < math.select(0 , 1 , enemySpawnTimerComponent.Timer <= 0f && PlayerCount > 0 && waveStateComponent.State == 1 && waveStockComponent.Stock > 0) ; i++)
            {
                // Necessary to sync Prefab choice with Tag choice
                int selection = randomComponent.Random.NextInt(0 , 3);

                // Necessary to capture the ID so we can move it
                // Inline Ternary: 0=Basic, 1=Fast, 2=Tank
                Entity newEnemy = EntityCommandBuffer.Instantiate(entityInQueryIndex , selection == 1 ? fastEnemyEntityComponent.Entity : (selection == 2 ? slowEnemyEntityComponent.Entity : basicEnemyEntityComponent.Entity));

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , LocalTransform.FromPosition(localTransform.Position + new float3(randomComponent.Random.NextFloat2Direction() * enemySpawnRadiusComponent.Radius , 0f)));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newEnemy , new MovementInputComponent { Input = float2.zero });

                // Adds BasicTag if selection == 0
                for(int k = 0 ; k < math.select(0 , 1 , selection == 0) ; k++) { EntityCommandBuffer.AddComponent<BasicEnemyTag>(entityInQueryIndex , newEnemy); }

                // Adds FastTag if selection == 1
                for(int k = 0 ; k < math.select(0 , 1 , selection == 1) ; k++) { EntityCommandBuffer.AddComponent<FastEnemyTag>(entityInQueryIndex , newEnemy); }

                // Adds SlowTag if selection == 2 and injects Turret Components for shooting
                for(int k = 0 ; k < math.select(0 , 1 , selection == 2) ; k++)
                {
                    EntityCommandBuffer.AddComponent<SlowEnemyTag>(entityInQueryIndex , newEnemy);

                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , bulletEntityComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , turretDamageComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , turretFireRateComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , turretProjectileCountComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , turretRangeComponent);
                    EntityCommandBuffer.AddComponent(entityInQueryIndex , newEnemy , turretSpreadComponent);
                }
            }

            waveStockComponent.Stock -= math.select(0 , 1 , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0);
            enemySpawnTimerComponent.Timer = math.select(enemySpawnTimerComponent.Timer , enemySpawnRateComponent.Rate , enemySpawnTimerComponent.Timer <= 0f && waveStateComponent.State == 1 && waveStockComponent.Stock > 0);
        }
    }
}