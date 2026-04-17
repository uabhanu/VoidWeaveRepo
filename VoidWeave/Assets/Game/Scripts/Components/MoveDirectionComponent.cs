namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Mathematics;
    
    public struct MoveDirectionComponent : IComponentData
    {
        public float3 Value;
    }
}