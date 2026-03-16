namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct MovementVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (movementVfxComponent , localTransform) in SystemAPI.Query<MovementVfxComponent , LocalTransform>().WithNone<DashVisualTag>())
            {
                Entity trailVfxEntity = ecb.Instantiate(movementVfxComponent.Value);
                ecb.SetComponent(trailVfxEntity , localTransform);
            }
        }
    }
}