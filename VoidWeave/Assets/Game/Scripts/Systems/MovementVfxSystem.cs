namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct MovementVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }
        
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (lastSpawnPositionComponent , localTransform , movementVfxComponent) in SystemAPI.Query<RefRW<LastSpawnPositionComponent> , LocalTransform , MovementVfxEntityComponent>().WithAll<LocalToWorld>().WithNone<DashVisualTag>())
            {
                float distanceMoved = math.distance(localTransform.Position , lastSpawnPositionComponent.ValueRO.Value);

                for(int i = 0 ; i < math.select(0 , 1 , distanceMoved > 0.2f) ; i++)
                {
                    Entity trailVfxEntity = ecb.Instantiate(movementVfxComponent.Value);

                    ecb.SetComponent(trailVfxEntity , localTransform);

                    var lifetimeData = SystemAPI.GetComponent<LifetimeComponent>(movementVfxComponent.Value);
                    ecb.SetComponentEnabled<VfxUpdateTag>(trailVfxEntity , true);
                    ecb.SetComponent(trailVfxEntity , lifetimeData);

                    lastSpawnPositionComponent.ValueRW.Value = localTransform.Position;
                }
            }
        }
    }
}