using Components;

namespace Systems
{
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct DashSystem : ISystem
    {
        private const float DASH_DURATION = 0.2f; // Length of dash
        private const float DASH_COOLDOWN = 1.0f; // Time before next dash
        private const float DASH_MULTIPLIER = 5.0f; // Speed boost (5 * 5 = 25 units/sec)

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            new DashJob { DeltaTime = deltaTime , DurationMax = DASH_DURATION , CooldownMax = DASH_COOLDOWN , SpeedMultiplier = DASH_MULTIPLIER }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct DashJob : IJobEntity
    {
        public float DeltaTime;
        public float DurationMax;
        public float CooldownMax;
        public float SpeedMultiplier;

        private void Execute(ref DashDurationComponent durationComponent , ref DashCooldownComponent cooldownComponent , ref MoveSpeedComponent moveSpeedComponent , in BaseMoveSpeedComponent baseSpeedComponent , in DashInputComponent dashInputComponent)
        {
            // Decrement Timers (Clamp to 0 to avoid negative numbers)
            // math.max(0, x) prevents the timer from going below zero
            float decreasedDuration = math.max(0f , durationComponent.Timer - DeltaTime);
            float decreasedCooldown = math.max(0f , cooldownComponent.Timer - DeltaTime);

            // Check Trigger Conditions (No Ifs)
            // Cooldown is finished (Timer <= 0) -> returns 1.0 if true
            float isCooldownReady = math.step(decreasedCooldown , 0f);

            // Input is Pressed -> returns 1.0 or 0.0
            float isInputPressed = dashInputComponent.IsPressed;

            // Trigger = Input * CooldownReady (Both must be 1.0)
            float startDash = isInputPressed * isCooldownReady;

            // Update Timers
            // If startDash is 1, set to MAX. If 0, keep the DECREMENTED value.
            // math.select(FalseValue, TrueValue, BooleanCondition)
            durationComponent.Timer = math.select(decreasedDuration , DurationMax , startDash > 0.5f);
            cooldownComponent.Timer = math.select(decreasedCooldown , CooldownMax , startDash > 0.5f);

            // Calculate Speed
            // Check if we are currently dashing (Duration > 0)
            float isDashing = math.step(0.001f , durationComponent.Timer);

            // Calculate Multiplier: (isDashing * 5) + (notDashing * 1)
            // If Dashing: (1 * 5) + (0 * 1) = 5
            // If Walking: (0 * 5) + (1 * 1) = 1
            float currentMultiplier = (isDashing * SpeedMultiplier) + (1.0f - isDashing);

            // Write Final Speed
            moveSpeedComponent.Speed = baseSpeedComponent.Speed * currentMultiplier;
        }
    }
}