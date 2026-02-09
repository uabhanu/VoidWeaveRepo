namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct VelocityComponent : IComponentData
    {
        public float2 Value;
    }
}