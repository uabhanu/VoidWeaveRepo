using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Game.Scripts.Components
{
    [MaterialProperty("_BaseColor")]
    public struct URPMaterialPropertyBaseColorComponent : IComponentData
    {
        public float4 Value;
    }
}