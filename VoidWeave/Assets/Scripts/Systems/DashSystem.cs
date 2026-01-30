namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct DashSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<DashCooldownDefaultComponent>();
            systemState.RequireForUpdate<DashDurationDefaultComponent>();
            systemState.RequireForUpdate<InputDashComponent>();
            systemState.RequireForUpdate<MovementActiveComponent>();
            systemState.RequireForUpdate<MovementNoneComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float dashCooldownDefault = SystemAPI.GetSingleton<DashCooldownDefaultComponent>().DashCooldownDefault;
            float dashDurationDefault = SystemAPI.GetSingleton<DashDurationDefaultComponent>().DashDurationDefault;
            uint inputDash = SystemAPI.GetSingleton<InputDashComponent>().InputDash;
            float movementActive = SystemAPI.GetSingleton<MovementActiveComponent>().MovementActive;
            float movementNone = SystemAPI.GetSingleton<MovementNoneComponent>().MovementNone;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Expired;
            
            systemState.Dependency = new DashJob { DashCooldownDefault = dashCooldownDefault , DeltaTime = SystemAPI.Time.DeltaTime , DashDurationDefault = dashDurationDefault , InputDashBit = inputDash , MovementActive = movementActive , MovementNone = movementNone , TimerExpired = timerExpired}.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct DashJob : IJobEntity
    {
        public float DashCooldownDefault;
        public float DashDurationDefault;
        public float DeltaTime;
        public uint InputDashBit;
        public float MovementActive;
        public float MovementNone;
        public float TimerExpired;

        private void Execute(in BaseMoveSpeedComponent baseMoveSpeedComponent , ref DashCooldownComponent dashCooldownComponent , ref DashDurationComponent dashDurationComponent , in DashMultiplierComponent dashMultiplierComponent , ref MoveSpeedComponent moveSpeedComponent , in PlayerInputComponent playerInputComponent)
        {
            bool isDashInputActive = (playerInputComponent.PlayerInput & InputDashBit) != (int)MovementNone;
            bool isCooldownReady = dashCooldownComponent.Timer <= TimerExpired;
            
            dashDurationComponent.Duration = math.select(math.max(TimerExpired , dashDurationComponent.Duration - DeltaTime) , DashDurationDefault , isDashInputActive && isCooldownReady);
            dashCooldownComponent.Timer = math.select(math.max(TimerExpired , dashCooldownComponent.Timer - DeltaTime) , DashCooldownDefault , isDashInputActive && isCooldownReady);
            
            moveSpeedComponent.Speed = baseMoveSpeedComponent.Speed * math.select(MovementActive , dashMultiplierComponent.Multiplier , dashDurationComponent.Duration > TimerExpired);
        }
    }
}