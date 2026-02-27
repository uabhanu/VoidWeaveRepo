namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Rendering;
    using UnityEngine.Rendering;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DashVisualSystem : ISystem
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
            
            float cullBack = (float)CullMode.Back;
            float cullFront = (float)CullMode.Front;

            foreach(var (materialMeshInfo , entity) in SystemAPI.Query<RefRW<MaterialMeshInfo>>().WithAll<DashVisualTag>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                bool isDashing = SystemAPI.IsComponentEnabled<DashVisualTag>(entity);
                
                int targetIndex = math.select(movementNone , movementActive , isDashing);
                materialMeshInfo.ValueRW = MaterialMeshInfo.FromRenderMeshArrayIndices(targetIndex , DefaultMeshIndex);
                
                float cullValue = math.select(cullBack , cullFront , isDashing);
                ecb.AddComponent(entity , new URPMaterialPropertyCull { Value = cullValue });

                var renderMeshArray = systemState.EntityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
                ecb.SetSharedComponentManaged(entity , renderMeshArray);
            }
        }
    }
}