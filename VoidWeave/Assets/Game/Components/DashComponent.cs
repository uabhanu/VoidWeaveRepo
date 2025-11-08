namespace Game.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct DashComponent : IComponentData
    {
        public float CooldownDuration;
        public float CurrentCooldown;
        public float2 DashDirection;
        public float DashDistance;
        public float DashDuration;
        public float DashTimer;
        public bool IsDashing;
    }
}