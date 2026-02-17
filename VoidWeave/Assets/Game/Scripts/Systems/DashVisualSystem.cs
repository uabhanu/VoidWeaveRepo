namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DashVisualSystem : ISystem
    {
        public void OnUpdate(ref SystemState systemState)
        {
            foreach(var (dashColorComponent , normalColorComponent , urpMaterialPropertyBaseColorComponent , entity) in SystemAPI.Query<DashColorComponent , NormalColorComponent , RefRW<URPMaterialPropertyBaseColorComponent>>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)) { urpMaterialPropertyBaseColorComponent.ValueRW.Value = SystemAPI.IsComponentEnabled<DashVisualTag>(entity) ? (float4)(UnityEngine.Vector4)dashColorComponent.Value : (float4)(UnityEngine.Vector4)normalColorComponent.Value; }
        }
    }
}