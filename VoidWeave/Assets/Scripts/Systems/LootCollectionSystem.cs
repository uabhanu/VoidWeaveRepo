namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct LootCollectionSystem : ISystem
    {
        private NativeQueue<int> _resourceQueue;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _resourceQueue = new NativeQueue<int>(Allocator.Persistent);
            
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<CurrentEnergyComponent>();
            state.RequireForUpdate<LocalTransform>();
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { _resourceQueue.Dispose(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new PickupJob { EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , PlayerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).Position , PickupRadiusSq = 0.5f * 0.5f , ResourceNativeQueueParallelWriter = _resourceQueue.AsParallelWriter() }.ScheduleParallel(state.Dependency).Complete();

            while(_resourceQueue.TryDequeue(out int value)) { SystemAPI.GetSingletonRW<CurrentEnergyComponent>().ValueRW.Energy += value; }
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