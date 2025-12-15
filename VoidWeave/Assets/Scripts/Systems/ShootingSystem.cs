namespace Systems
{
    using Components;
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
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var targetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent>().Build();
            var targetTransforms = targetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out var gatherHandle1);
            var targetTeams = targetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob , out var gatherHandle2);

            var combinedDependency = JobHandle.CombineDependencies(state.Dependency , gatherHandle1 , gatherHandle2);
            var jobHandle = new TurretShootJob { DeltaTime = deltaTime , TargetTeams = targetTeams , TargetPositions = targetTransforms , EntityCommandBuffer = ecb }.ScheduleParallel(combinedDependency);

            targetTeams.Dispose(jobHandle);
            targetTransforms.Dispose(jobHandle);

            state.Dependency = jobHandle;
        }
    }

    [BurstCompile]
    public partial struct TurretShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public float DeltaTime;
        [ReadOnly] public NativeList<TeamComponent> TargetTeams;
        [ReadOnly] public NativeList<LocalToWorld> TargetPositions;

        private void Execute(in BulletEntityComponent bulletEntityComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalToWorld localToWorld , in TeamComponent teamComponent , ref TurretCooldownComponent turretCooldownComponent , in TurretDamageComponent turretDamageComponent , in TurretFireRateComponent turretFireRateComponent , in TurretProjectileCountComponent turretProjectileCountComponent , in TurretRangeComponent turretRangeComponent , in TurretSpreadComponent turretSpreadComponent)
        {
            turretCooldownComponent.Timer -= DeltaTime;

            float isCooldownReady = math.step(turretCooldownComponent.Timer , 0f);

            float3 turretPos = localToWorld.Position;
            float closestDistSq = float.MaxValue;
            float3 bestTargetPos = float3.zero;

            float foundTarget = 0f;

            for(int i = 0 ; i < TargetPositions.Length ; i++)
            {
                float3 currentTargetPos = TargetPositions[i].Position;
                float distSq = math.distancesq(turretPos , currentTargetPos);

                bool isDifferentTeam = TargetTeams[i].ID != teamComponent.ID;

                // Ghost Check: (Position is not 0,0,0). This is a compromise that is worth it.
                bool isNotGhost = math.lengthsq(currentTargetPos) > 0.001f;

                // Distance Check: (Current < ClosestSoFar)
                bool isCloserDist = distSq < closestDistSq;

                // COMBINE: Valid only if ALL are true
                bool isValidTarget = isDifferentTeam & isNotGhost & isCloserDist;

                closestDistSq = math.select(closestDistSq , distSq , isValidTarget);
                bestTargetPos = math.select(bestTargetPos , currentTargetPos , isValidTarget);
                foundTarget = math.select(foundTarget , 1f , isValidTarget);
            }

            float rangeSq = turretRangeComponent.Range * turretRangeComponent.Range;
            float isWithinRange = math.step(closestDistSq , rangeSq);
            float shouldFire = isCooldownReady * foundTarget * isWithinRange;

            turretCooldownComponent.Timer = math.select(turretCooldownComponent.Timer , turretFireRateComponent.Rate , shouldFire > 0.5f);

            int projectileCount = turretProjectileCountComponent.Count;
            int totalFireCount = (int)shouldFire * projectileCount;

            float divisor = math.max(1 , projectileCount - 1);
            float totalSpreadRad = math.radians(turretSpreadComponent.Degrees);
            float angleStep = totalSpreadRad / divisor;

            float3 directionToTarget = math.normalizesafe(bestTargetPos - turretPos);
            float baseAngle = math.atan2(directionToTarget.y , directionToTarget.x);
            float startAngle = baseAngle - (totalSpreadRad * 0.5f);

            for(int i = 0 ; i < totalFireCount ; i++)
            {
                Entity newBullet = EntityCommandBuffer.Instantiate(entityInQueryIndex , bulletEntityComponent.Entity);
                
                float currentAngle = startAngle + angleStep * i;
                float visualAngle = currentAngle - math.PI / 2f;
                
                quaternion rotation = quaternion.RotateZ(visualAngle);
                float2 moveDirection = new float2(math.cos(currentAngle) , math.sin(currentAngle));

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , LocalTransform.FromPositionRotation(turretPos , rotation));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , new MovementInputComponent { Input = moveDirection });
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , new ProjectileDamageComponent { Damage = turretDamageComponent.Damage });
            }
        }
    }
}