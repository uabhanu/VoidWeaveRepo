namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DashVisualSystem : ISystem
    {
        public void OnUpdate(ref SystemState systemState)
        {
            foreach((DashColorComponent dashColorComponent , NormalColorComponent normalColorComponent , RefRW<URPMaterialPropertyBaseColorComponent> urpMaterialPropertyBaseColorComponent , Entity entity) in SystemAPI.Query<DashColorComponent , NormalColorComponent , RefRW<URPMaterialPropertyBaseColorComponent>>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)) urpMaterialPropertyBaseColorComponent.ValueRW.Value = SystemAPI.IsComponentEnabled<DashVisualTag>(entity) ? (Vector4)dashColorComponent.Value : (float4)(Vector4)normalColorComponent.Value;
        }
    }
}