namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DamageVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (damageVfxComponent , localTransform , vfxColorComponent , vfxSizeComponent , entity) in SystemAPI.Query<DamageVfxComponent , LocalTransform , VfxColorComponent , VfxSizeComponent>().WithAll<DamageTag>().WithEntityAccess())
            {
                Entity instance = ecb.Instantiate(damageVfxComponent.Value);
                ecb.SetComponent(instance , localTransform);
                ecb.SetComponent(instance , SystemAPI.GetComponent<LifetimeComponent>(damageVfxComponent.Value));

                ecb.AddComponent(instance , vfxColorComponent);
                ecb.AddComponent(instance , vfxSizeComponent);
                ecb.AddComponent(instance , new VfxUpdateTag());

                ecb.RemoveComponent<DamageTag>(entity);
            }
        }
    }
}