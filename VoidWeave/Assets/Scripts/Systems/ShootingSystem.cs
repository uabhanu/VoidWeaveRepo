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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var targetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent>().Build();
            var targetTransforms = targetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out var gatherHandle1);
            var targetTeams = targetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob, out var gatherHandle2);

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

        private void Execute(in BulletEntityComponent bulletEntityComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalToWorld localToWorld , in TeamComponent teamComponent , ref TurretCooldownComponent turretCooldownComponent , in TurretFireRateComponent turretFireRateComponent , in TurretRangeComponent turretRangeComponent)
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

                // 2. Ghost Check: (Position is not 0,0,0). This is a compromise that is worth it.
                bool isNotGhost = math.lengthsq(currentTargetPos) > 0.001f;

                // 3. Distance Check: (Current < ClosestSoFar)
                bool isCloserDist = distSq < closestDistSq;

                // 4. COMBINE: Valid only if ALL are true
                bool isValidTarget = isDifferentTeam & isNotGhost & isCloserDist;

                closestDistSq = math.select(closestDistSq , distSq , isValidTarget);
                bestTargetPos = math.select(bestTargetPos , currentTargetPos , isValidTarget);
                foundTarget = math.select(foundTarget , 1f , isValidTarget);
            }

            float rangeSq = turretRangeComponent.Range * turretRangeComponent.Range;
            float isWithinRange = math.step(closestDistSq , rangeSq);
            float shouldFire = isCooldownReady * foundTarget * isWithinRange;

            turretCooldownComponent.Timer = math.select(turretCooldownComponent.Timer , turretFireRateComponent.Rate , shouldFire > 0.5f);

            int fireCount = (int)shouldFire;

            for(int i = 0 ; i < fireCount ; i++)
            {
                Entity newBullet = EntityCommandBuffer.Instantiate(entityInQueryIndex , bulletEntityComponent.BulletEntity);
                float3 direction = math.normalizesafe(bestTargetPos - turretPos);
                float angle = math.atan2(direction.y , direction.x) - math.PI / 2f;
                quaternion rotation = quaternion.RotateZ(angle);

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , LocalTransform.FromPositionRotation(turretPos , rotation));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , new MovementInputComponent { MoveInput = direction.xy });
            }
        }
    }
}