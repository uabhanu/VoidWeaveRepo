namespace Game.Components
{
    using Unity.Entities;
    using Unity.Mathematics;
    
    public struct PlayerMovementComponent : IComponentData
    {
        public float2 CurrentVelocity;
        public float MoveSpeed;
    }
}