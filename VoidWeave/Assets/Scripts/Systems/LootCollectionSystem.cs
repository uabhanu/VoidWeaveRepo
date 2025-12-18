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
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct LootCollectionSystem : ISystem
    {
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
            NativeQueue<int> resourceNativeQueue = new NativeQueue<int>(Allocator.TempJob);
            
            JobHandle jobHandle = new PickupJob { EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , PlayerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).Position , PickupRadiusSq = 0.5f * 0.5f , ResourceNativeQueueParallelWriter = resourceNativeQueue.AsParallelWriter() }.ScheduleParallel(state.Dependency);

            jobHandle.Complete();
            
            while(resourceNativeQueue.TryDequeue(out int value)) { SystemAPI.GetSingletonRW<CurrentEnergyComponent>().ValueRW.Energy += value; }

            resourceNativeQueue.Dispose();
        }
    }

    [BurstCompile]
    [WithAll(typeof(LootPickupTag))]
    public partial struct PickupJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public float3 PlayerPos;
        public float PickupRadiusSq;
        public NativeQueue<int>.ParallelWriter ResourceNativeQueueParallelWriter;
        
        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in LootAmountComponent lootAmountComponent)
        {
            for(int i = 0 ; i < math.select(0 , 1 , math.distancesq(localTransform.Position , PlayerPos) <= PickupRadiusSq) ; i++)
            {
                EntityCommandBufferParallelWriter.DestroyEntity(entityInQueryIndex , entity);
                ResourceNativeQueueParallelWriter.Enqueue(lootAmountComponent.Amount);
            }
        }
    }
}