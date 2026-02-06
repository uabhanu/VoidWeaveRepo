namespace Components
{
    using Unity.Entities;

    public struct TimerExpiredComponent : IComponentData
    {
        public float Value;
    }
}