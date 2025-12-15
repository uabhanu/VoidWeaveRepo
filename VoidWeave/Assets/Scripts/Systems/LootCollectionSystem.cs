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
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct LootCollectionSystem : ISystem
    {
        private const float PICKUP_RADIUS = 0.5f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<CurrentEnergyComponent>();
            state.RequireForUpdate<LocalTransform>();
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get the specific Player entity
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            float3 playerPos = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            NativeQueue<int> resourceQueue = new NativeQueue<int>(Allocator.TempJob);
            NativeQueue<int>.ParallelWriter resourceQueueWriter = resourceQueue.AsParallelWriter();

            // Schedule the job
            JobHandle jobHandle = new PickupJob { EntityCommandBuffer = ecb , PlayerPos = playerPos , PickupRadiusSq = PICKUP_RADIUS * PICKUP_RADIUS , ResourceQueueWriter = resourceQueueWriter }.ScheduleParallel(state.Dependency);

            // Complete immediately to process queue
            jobHandle.Complete();

            int totalGained = 0;
            while(resourceQueue.TryDequeue(out int value)) { totalGained += value; }

            RefRW<CurrentEnergyComponent> energy = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            energy.ValueRW.Energy += totalGained;

            resourceQueue.Dispose();
            state.Dependency = default;
        }
    }

    [BurstCompile]
    [WithAll(typeof(LootPickupTag))]
    public partial struct PickupJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public float3 PlayerPos;
        public float PickupRadiusSq;
        public NativeQueue<int>.ParallelWriter ResourceQueueWriter;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , Entity entity , in LocalTransform localTransform , in LootAmountComponent lootAmount)
        {
            float distSq = math.distancesq(localTransform.Position , PlayerPos);

            float isPickedUp = math.step(distSq , PickupRadiusSq);

            int pickupCount = (int)isPickedUp;

            for(int i = 0 ; i < pickupCount ; i++)
            {
                EntityCommandBuffer.DestroyEntity(entityInQueryIndex , entity);
                ResourceQueueWriter.Enqueue(lootAmount.Amount);
            }
        }
    }
}