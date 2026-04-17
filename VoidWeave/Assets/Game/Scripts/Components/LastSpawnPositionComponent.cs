namespace Game.Scripts.Components
{
    using Unity.Mathematics;
    using Unity.Entities;

    public struct LastSpawnPositionComponent : IComponentData
    {
        public float3 Value;
    }
}