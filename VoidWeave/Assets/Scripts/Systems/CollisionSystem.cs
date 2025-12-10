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
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _targetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            // Query for anything that is a "Target" (Enemies)
            _targetQuery = SystemAPI.QueryBuilder().WithAll<TurretTargetTag , LocalToWorld>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Setup Command Buffer
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Collect Targets
            // We need Entities (to destroy them) and Positions (to check distance)
            NativeList<Entity> targetEntities = _targetQuery.ToEntityListAsync(Allocator.TempJob , out JobHandle targetEntityHandle);
            NativeList<LocalToWorld> targetPositions = _targetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out JobHandle targetPosHandle);

            // Combine Dependencies
            JobHandle dependency = JobHandle.CombineDependencies(state.Dependency , targetEntityHandle , targetPosHandle);

            // Schedule Job
            JobHandle jobHandle = new CollisionJob
            {
                EntityCommandBuffer = ecb , TargetEntities = targetEntities , TargetPositions = targetPositions , HitRadiusSq = 0.5f * 0.5f // Threshold squared (0.5 units)
            }.ScheduleParallel(dependency);

            // Dispose Lists after Job
            targetEntities.Dispose(jobHandle);
            targetPositions.Dispose(jobHandle);

            state.Dependency = jobHandle;
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileTag))] // Only iterate over Bullets
    public partial struct CollisionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        [ReadOnly] public NativeList<Entity> TargetEntities;
        [ReadOnly] public NativeList<LocalToWorld> TargetPositions;

        public float HitRadiusSq;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , Entity bulletEntity , in LocalToWorld bulletTransform)
        {
            float3 bulletPos = bulletTransform.Position;

            // Branchless Logic Variables
            float hasHit = 0f;
            int targetIndexToDestroy = 0;

            // Iterate through all cached targets
            for(int i = 0 ; i < TargetPositions.Length ; i++)
            {
                float3 targetPos = TargetPositions[i].Position;
                float distSq = math.distancesq(bulletPos , targetPos);

                // Check if distance is within radius (1.0 = Hit, 0.0 = Miss)
                float isHit = math.step(distSq , HitRadiusSq);

                // We only want to process the FIRST hit to avoid logic errors or double counting.
                // isFirstHit will be 1 only if (isHit == 1) AND (hasHit == 0)
                float isFirstHit = isHit * (1f - hasHit);

                // If this is the first hit, store the index of this target.
                // If not, keep the previous targetIndexToDestroy.
                targetIndexToDestroy = math.select(targetIndexToDestroy , i , isFirstHit > 0.5f);

                // Update hasHit status (once it becomes 1, it stays 1)
                hasHit = math.max(hasHit , isHit);
            }

            // Convert float flag to int count (0 or 1)
            int destroyCount = (int)hasHit;

            // Execute Destruction
            for(int k = 0 ; k < destroyCount ; k++)
            {
                EntityCommandBuffer.DestroyEntity(entityInQueryIndex , bulletEntity);
                EntityCommandBuffer.DestroyEntity(entityInQueryIndex , TargetEntities[targetIndexToDestroy]);
            }
        }
    }
}