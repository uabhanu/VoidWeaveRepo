namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DamageVfxSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach((DamageVfxComponent vfx , LocalTransform transform , Entity entity) in SystemAPI.Query<DamageVfxComponent , LocalTransform>().WithAll<DamageTag>().WithEntityAccess())
            {
                Entity instance = ecb.Instantiate(vfx.Value);
                ecb.SetComponent(instance , LocalTransform.FromPosition(transform.Position));
                ecb.RemoveComponent<DamageTag>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}