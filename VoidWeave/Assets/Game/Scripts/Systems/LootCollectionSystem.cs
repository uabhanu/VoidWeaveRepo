namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct LootCollectionSystem : ISystem
    {
        private NativeQueue<int> _resourceQueue;
        
        public void OnCreate(ref SystemState systemState)
        {
            _resourceQueue = new NativeQueue<int>(Allocator.Persistent);

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<LocalTransform>();
            systemState.RequireForUpdate<LootPickupRadiusComponent>();

            systemState.RequireForUpdate<PlayerTag>();
        }
        
        public void OnDestroy(ref SystemState state) { _resourceQueue.Dispose(); }
        
        public void OnUpdate(ref SystemState state)
        {
            new PickupJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , PlayerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).Position , PickupRadiusSq = SystemAPI.GetSingleton<LootPickupRadiusComponent>().Value * SystemAPI.GetSingleton<LootPickupRadiusComponent>().Value , ResourceNativeQueueParallelWriter = _resourceQueue.AsParallelWriter() }.ScheduleParallel(state.Dependency).Complete();

            while(_resourceQueue.TryDequeue(out int value)) SystemAPI.GetSingletonRW<CurrentEnergyComponent>().ValueRW.Value += value;
        }
    }

    [BurstCompile]
    [WithAll(typeof(LootPickupTag))]
    public partial struct PickupJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float3 PlayerPos;
        public float PickupRadiusSq;
        public NativeQueue<int>.ParallelWriter ResourceNativeQueueParallelWriter;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in LootAmountComponent lootAmountComponent)
        {
            for(var i = 0 ; i < math.select(0 , 1 , math.distancesq(localTransform.Position , PlayerPos) <= PickupRadiusSq) ; i++)
            {
                ECB.DestroyEntity(entityInQueryIndex , entity);
                ResourceNativeQueueParallelWriter.Enqueue(lootAmountComponent.Value);
            }
        }
    }
}