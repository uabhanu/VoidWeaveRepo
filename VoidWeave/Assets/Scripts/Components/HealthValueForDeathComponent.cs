namespace Components
{
    using Unity.Entities;

    public struct HealthValueForDeathComponent : IComponentData
    {
        public float Value;
    }
}