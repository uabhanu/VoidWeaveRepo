using Components;

namespace Systems
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _targetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<CurrentEnergyComponent>();
            
            // Query for anything that is a "Target"
            _targetQuery = SystemAPI.QueryBuilder().WithAll<TurretTargetTag , LocalToWorld , LootAmountComponent , LootEntityComponent>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Setup Command Buffer
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Collect Targets
            // We need Entities (to destroy them) and Positions (to check distance)
            NativeList<Entity> targetEntitiesNativeList = _targetQuery.ToEntityListAsync(Allocator.TempJob , out JobHandle targetEntityJobHandle);
            NativeList<LootAmountComponent> lootAmountsNativeList = _targetQuery.ToComponentDataListAsync<LootAmountComponent>(Allocator.TempJob , out JobHandle lootAmountJobHandle);
            NativeList<LootEntityComponent> lootEntitiesNativeList = _targetQuery.ToComponentDataListAsync<LootEntityComponent>(Allocator.TempJob , out JobHandle lootEntityJobHandle);
            NativeList<LocalToWorld> targetPositionsNativeList = _targetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out JobHandle targetPosJobHandle);

            // Combine Dependencies
            JobHandle dependencyJobHandle = JobHandle.CombineDependencies(state.Dependency , targetEntityJobHandle , targetPosJobHandle);
            dependencyJobHandle = JobHandle.CombineDependencies(dependencyJobHandle , lootEntityJobHandle);
            dependencyJobHandle = JobHandle.CombineDependencies(dependencyJobHandle , lootAmountJobHandle);

            // Schedule Job
            JobHandle collisionJobHandle = new CollisionJob
            {
                EntityCommandBuffer = ecb , LootAmountsNativeList = lootAmountsNativeList , LootEntitiesNativeList = lootEntitiesNativeList , TargetEntities = targetEntitiesNativeList , TargetPositions = targetPositionsNativeList , HitRadiusSq = 0.5f * 0.5f // Threshold squared (0.5 units)
            }.ScheduleParallel(dependencyJobHandle);
            
            collisionJobHandle.Complete();

            // Dispose Lists after Job
            lootAmountsNativeList.Dispose(collisionJobHandle);
            lootEntitiesNativeList.Dispose(collisionJobHandle);
            targetEntitiesNativeList.Dispose(collisionJobHandle);
            targetPositionsNativeList.Dispose(collisionJobHandle);

            state.Dependency = collisionJobHandle;
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileTag))] // Only iterate over Bullets
    public partial struct CollisionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        [ReadOnly] public NativeList<LootEntityComponent> LootEntitiesNativeList;
        [ReadOnly] public NativeList<LootAmountComponent> LootAmountsNativeList;
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
            
            for(int k = 0 ; k < destroyCount ; k++)
            {
                EntityCommandBuffer.DestroyEntity(entityInQueryIndex , bulletEntity);
                EntityCommandBuffer.DestroyEntity(entityInQueryIndex , TargetEntities[targetIndexToDestroy]);
                
                Entity newDrop = EntityCommandBuffer.Instantiate(entityInQueryIndex , LootEntitiesNativeList[targetIndexToDestroy].Entity);
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newDrop , LocalTransform.FromPosition(TargetPositions[targetIndexToDestroy].Position));
                
                int specificAmount = LootAmountsNativeList[targetIndexToDestroy].Amount;
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newDrop , new LootAmountComponent { Amount = specificAmount });
            }
        }
    }
}