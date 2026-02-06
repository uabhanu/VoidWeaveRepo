namespace Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct RandomSeedComponent : IComponentData
    {
        public Random Value;
    }
}