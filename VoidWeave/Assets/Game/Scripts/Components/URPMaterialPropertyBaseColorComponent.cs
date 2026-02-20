namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Rendering;

    [MaterialProperty("_BaseColor")]
    public struct URPMaterialPropertyBaseColorComponent : IComponentData
    {
        public float4 Value;
    }
}