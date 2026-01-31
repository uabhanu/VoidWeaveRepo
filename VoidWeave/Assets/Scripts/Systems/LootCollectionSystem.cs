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
        public void OnCreate(ref SystemState systemState)
        {
            _resourceQueue = new NativeQueue<int>(Allocator.Persistent);
            
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<LocalTransform>();
            systemState.RequireForUpdate<LootPickupRadiusComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            
            systemState.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { _resourceQueue.Dispose(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().DoAction;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().NoActionValue;
            float pickupRadius = SystemAPI.GetSingleton<LootPickupRadiusComponent>().Radius;
            
            new PickupJob { DoAction = doAction , EntityCommandBufferParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , NoAction = noAction , PlayerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).Position , PickupRadiusSq = pickupRadius * pickupRadius , ResourceNativeQueueParallelWriter = _resourceQueue.AsParallelWriter() }.ScheduleParallel(state.Dependency).Complete();

            while(_resourceQueue.TryDequeue(out int value)) { SystemAPI.GetSingletonRW<CurrentEnergyComponent>().ValueRW.Energy += value; }
        }
    }

    [BurstCompile]
    [WithAll(typeof(LootPickupTag))]
    public partial struct PickupJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter EntityCommandBufferParallelWriter;
        public int NoAction;
        public float3 PlayerPos;
        public float PickupRadiusSq;
        public NativeQueue<int>.ParallelWriter ResourceNativeQueueParallelWriter;
        
        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in LootAmountComponent lootAmountComponent)
        {
            for(int i = 0 ; i < math.select(NoAction , DoAction , math.distancesq(localTransform.Position , PlayerPos) <= PickupRadiusSq) ; i++)
            {
                EntityCommandBufferParallelWriter.DestroyEntity(entityInQueryIndex , entity);
                ResourceNativeQueueParallelWriter.Enqueue(lootAmountComponent.Amount);
            }
        }
    }
}