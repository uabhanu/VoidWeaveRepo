namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct VfxScaleComponent : IComponentData
    {
        public float3 Value;
    }
}