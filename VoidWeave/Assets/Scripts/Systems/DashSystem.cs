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
        public void OnUpdate(ref SystemState state) { state.Dependency = new DashJob { CooldownMax = 1.0f , DeltaTime = SystemAPI.Time.DeltaTime , DurationMax = 0.2f }.ScheduleParallel(state.Dependency); }
    }

    [BurstCompile]
    public partial struct DashJob : IJobEntity
    {
        public float CooldownMax;
        public float DeltaTime;
        public float DurationMax;

        private void Execute(in BaseMoveSpeedComponent baseMoveSpeedComponent , ref DashCooldownComponent dashCooldownComponent , ref DashDurationComponent dashDurationComponent , in PlayerInputComponent playerInputComponent , in DashMultiplierComponent dashMultiplierComponent , ref MoveSpeedComponent moveSpeedComponent)
        {
            // DO NOT CHANGE THE ORDER OF THE FOLLOWING LINES
            // SelectedInput & 16 corresponds to the Dash input bit defined in InputSystem
            dashDurationComponent.Duration = math.select(math.max(0f , dashDurationComponent.Duration - DeltaTime) , DurationMax , (playerInputComponent.SelectedInput & 16) != 0 && dashCooldownComponent.Timer <= 0f);
            dashCooldownComponent.Timer = math.select(math.max(0f , dashCooldownComponent.Timer - DeltaTime) , CooldownMax , (playerInputComponent.SelectedInput & 16) != 0 && dashCooldownComponent.Timer <= 0f);
            moveSpeedComponent.Speed = baseMoveSpeedComponent.Speed * math.select(1.0f , dashMultiplierComponent.Multiplier , dashDurationComponent.Duration > 0f);
        }
    }
}