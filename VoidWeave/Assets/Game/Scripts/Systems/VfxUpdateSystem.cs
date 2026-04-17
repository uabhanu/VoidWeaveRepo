namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine.VFX;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct VfxUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach(var (vfxColor , vfxScale , vfxSize , entity) in SystemAPI.Query<RefRO<VfxColorComponent> , RefRO<VfxScaleComponent> , RefRO<VfxSizeComponent>>().WithAll<VfxUpdateTag>().WithEntityAccess())
            {
                var vfxMeshComponent = systemState.EntityManager.GetComponentData<VfxMeshComponent>(entity);
                var vfxTextureComponent = systemState.EntityManager.GetComponentData<VfxTextureComponent>(entity);
                var visualEffect = systemState.EntityManager.GetComponentObject<VisualEffect>(entity);

                visualEffect.SetVector3("Color" , vfxColor.ValueRO.Value);
                visualEffect.SetMesh("Mesh" , vfxMeshComponent.Value);
                visualEffect.SetVector3("Scale" , vfxScale.ValueRO.Value);
                visualEffect.SetFloat("Size" , vfxSize.ValueRO.Value);
                visualEffect.SetTexture("Texture" , vfxTextureComponent.Value);

                entityCommandBuffer.RemoveComponent<VfxUpdateTag>(entity);
            }
        }
    }
}