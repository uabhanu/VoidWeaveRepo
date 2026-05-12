namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct SpawnVfxSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<OneScaleComponent>();
            systemState.RequireForUpdate<ZeroScaleComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            float oneScale = SystemAPI.GetSingleton<OneScaleComponent>().Value;
            float zeroScale = SystemAPI.GetSingleton<ZeroScaleComponent>().Value;
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (localTransform , timerComponent , entity) in SystemAPI.Query<RefRW<LocalTransform> , RefRW<TimerComponent>>().WithAll<SpawningTag>().WithEntityAccess())
            {
                timerComponent.ValueRW.Value -= deltaTime;

                float progress = math.saturate(oneScale - timerComponent.ValueRO.Value * oneScale);
                localTransform.ValueRW.Scale = progress;
                
                ecb.SetComponentEnabled<SpawningTag>(entity , timerComponent.ValueRO.Value > zeroScale);
            }
        }
    }
}