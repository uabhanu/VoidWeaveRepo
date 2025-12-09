namespace Gameplay
{
    using Unity.Entities;
    using Unity.Mathematics;
    
    #region Components
    
    public struct BaseMoveSpeedComponent : IComponentData
    {
        public float BaseSpeed;
    }

    public struct DashCooldownComponent : IComponentData
    {
        public float Timer;
    }

    public struct DashDurationComponent : IComponentData
    {
        public float Timer;
    }

    public struct DashInputComponent : IComponentData
    {
        public float IsPressed;
    }

    public struct MoveSpeedComponent : IComponentData
    {
        public float MoveSpeed;
    }

    public struct MovementInputComponent : IComponentData
    {
        public float2 MoveInput;
    }
    
    #endregion
    
    #region Tags

    public struct PlayerTag : IComponentData {}
    
    #endregion
}