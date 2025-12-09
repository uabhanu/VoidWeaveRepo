namespace Systems
{
    using Gameplay;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ShootingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BulletPrefabComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            Entity bulletPrefab = SystemAPI.GetSingleton<BulletPrefabComponent>().BulletPrefab;

            var enemyQuery = SystemAPI.QueryBuilder().WithAll<TurretTargetTag , LocalToWorld>().Build();
            var enemyTransforms = enemyQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out var gatherHandle);

            var combinedDependency = JobHandle.CombineDependencies(state.Dependency , gatherHandle);
            var jobHandle = new TurretShootJob { BulletPrefab = bulletPrefab , DeltaTime = deltaTime , EnemyPositions = enemyTransforms , EntityCommandBuffer = ecb }.ScheduleParallel(combinedDependency);

            enemyTransforms.Dispose(jobHandle);

            state.Dependency = jobHandle;
        }
    }

    [BurstCompile]
    public partial struct TurretShootJob : IJobEntity
    {
        public Entity BulletPrefab;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public float DeltaTime;
        [ReadOnly] public NativeList<LocalToWorld> EnemyPositions;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , ref TurretCooldownComponent turretCooldownComponent , in TurretRangeComponent turretRangeComponent , in TurretFireRateComponent turretFireRateComponent , in LocalToWorld localToWorld)
        {
            turretCooldownComponent.Timer -= DeltaTime;

            float isCooldownReady = math.step(turretCooldownComponent.Timer , 0f);

            float3 turretPos = localToWorld.Position;
            float closestDistSq = float.MaxValue;
            float3 targetPos = float3.zero;

            float foundTarget = 0f;

            for(int i = 0 ; i < EnemyPositions.Length ; i++)
            {
                float3 enemyPos = EnemyPositions[i].Position;
                float distSq = math.distancesq(turretPos , enemyPos);
                float isCloser = math.step(distSq , closestDistSq);

                closestDistSq = math.select(closestDistSq , distSq , isCloser > 0.5f);
                targetPos = math.select(targetPos , enemyPos , isCloser > 0.5f);
                foundTarget = 1f;
            }

            float rangeSq = turretRangeComponent.Range * turretRangeComponent.Range;
            float isWithinRange = math.step(closestDistSq , rangeSq);
            float shouldFire = isCooldownReady * foundTarget * isWithinRange;

            turretCooldownComponent.Timer = math.select(turretCooldownComponent.Timer , turretFireRateComponent.Rate , shouldFire > 0.5f);

            int fireCount = (int)shouldFire;

            for(int i = 0 ; i < fireCount ; i++)
            {
                Entity newBullet = EntityCommandBuffer.Instantiate(entityInQueryIndex , BulletPrefab);
                float3 direction = math.normalizesafe(targetPos - turretPos);
                float angle = math.atan2(direction.y , direction.x) - math.PI / 2f;
                quaternion rotation = quaternion.RotateZ(angle);

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , LocalTransform.FromPositionRotation(turretPos , rotation));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , new MovementInputComponent { MoveInput = direction.xy });
            }
        }
    }
}