namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct DashVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (dashVfx , localTransform) in SystemAPI.Query<DashVfxComponent , LocalTransform>().WithAll<DashVisualTag>())
            {
                Entity dashTrail = ecb.Instantiate(dashVfx.Value);
                ecb.AddComponent<VfxUpdateTag>(dashTrail);
                ecb.SetComponent(dashTrail , localTransform);
                ecb.SetComponent(dashTrail , SystemAPI.GetComponent<LifetimeComponent>(dashVfx.Value));
            }
        }
    }
}