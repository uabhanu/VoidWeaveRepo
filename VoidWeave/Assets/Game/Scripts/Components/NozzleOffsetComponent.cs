namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct NozzleOffsetComponent : IComponentData
    {
        public float3 Value;
    }
}