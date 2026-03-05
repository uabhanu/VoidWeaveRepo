namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct ProjectileSpawnPointComponent : IComponentData
    {
        public float3 Value;
    }
}