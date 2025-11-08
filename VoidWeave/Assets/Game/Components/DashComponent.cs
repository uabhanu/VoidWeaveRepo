namespace Game.Components
{
    using System.Runtime.InteropServices;
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
        [MarshalAs(UnmanagedType.U1)] public bool IsDashing;
    }
}