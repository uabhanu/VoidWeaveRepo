namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct SpawnVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState systemState)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (localTransform , timerComponent , entity) in SystemAPI.Query<RefRW<LocalTransform> , RefRW<TimerComponent>>().WithAll<SpawningVfxTag>().WithEntityAccess())
            {
                timerComponent.ValueRW.Value -= deltaTime;

                float progress = math.saturate(1f - timerComponent.ValueRO.Value * 1f);
                localTransform.ValueRW.Scale = progress;
                
                ecb.SetComponentEnabled<SpawningVfxTag>(entity , timerComponent.ValueRO.Value > 0f);
            }
        }
    }
}