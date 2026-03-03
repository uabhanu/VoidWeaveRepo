namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Rendering;
    using Unity.Transforms;
    using UnityEngine.Rendering;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DashVfxSystem : ISystem
    {
        private const int DefaultMeshIndex = 0;

        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<MovementActiveComponent>();
            systemState.RequireForUpdate<MovementNoneComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            int movementActive = (int)SystemAPI.GetSingleton<MovementActiveComponent>().Value;
            int movementNone = (int)SystemAPI.GetSingleton<MovementNoneComponent>().Value;

            foreach(var (dashVfx , localTransform , materialMeshInfo , entity) in SystemAPI.Query<DashVfxComponent , LocalTransform , RefRW<MaterialMeshInfo>>().WithAll<DashVisualTag>().WithEntityAccess())
            {
                Entity dashTrail = ecb.Instantiate(dashVfx.Value);
                ecb.SetComponent(dashTrail , localTransform);
                ecb.SetComponent(dashTrail , SystemAPI.GetComponent<LifetimeComponent>(dashVfx.Value));

                materialMeshInfo.ValueRW = MaterialMeshInfo.FromRenderMeshArrayIndices(movementActive , DefaultMeshIndex);
                ecb.AddComponent(entity , new URPMaterialPropertyCull { Value = (float)CullMode.Front });

                var renderMeshArray = systemState.EntityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
                ecb.SetSharedComponentManaged(entity , renderMeshArray);
            }
            
            //dashVfx variable is a dummy here but needs to stay here
            foreach(var (dashVfx , materialMeshInfo , entity) in SystemAPI.Query<DashVfxComponent , RefRW<MaterialMeshInfo>>().WithNone<DashVisualTag>().WithEntityAccess())
            {
                materialMeshInfo.ValueRW = MaterialMeshInfo.FromRenderMeshArrayIndices(movementNone , DefaultMeshIndex);
                ecb.AddComponent(entity , new URPMaterialPropertyCull { Value = (float)CullMode.Back });

                var renderMeshArray = systemState.EntityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
                ecb.SetSharedComponentManaged(entity , renderMeshArray);
            }
        }
    }
}