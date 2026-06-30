namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct DamageVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }
        
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            
            foreach(var (damageVfxComponent , localTransform , vfxColorComponent , vfxMeshComponent , vfxScaleComponent , vfxSizeComponent , vfxTextureComponent , entity) in SystemAPI.Query<DamageVfxComponent , LocalTransform , VfxColorComponent , VfxMeshComponent , VfxScaleComponent , VfxSizeComponent , VfxTextureComponent>().WithAll<DamageTag>().WithEntityAccess())
            { 
                ecb.SetComponentEnabled<DamageTag>(entity , false);
                
                // Instantiate immediately
                Entity entityInstance = ecb.Instantiate(damageVfxComponent.Value);

                // Set all data using ECB.SetComponent
                ecb.SetComponent(entityInstance , vfxColorComponent);
                ecb.SetComponent(entityInstance , new VfxScaleComponent { Value = vfxScaleComponent.Value });
                ecb.SetComponent(entityInstance , vfxSizeComponent);
                ecb.SetComponent(entityInstance , localTransform);
                ecb.SetComponent(entityInstance , vfxMeshComponent);
                ecb.SetComponent(entityInstance , vfxTextureComponent);

                // Turn the VFX On
                ecb.SetComponentEnabled<VfxUpdateTag>(entityInstance , true);
            }
        }
    }
}