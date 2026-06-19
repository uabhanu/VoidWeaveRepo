namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct DashSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<DashCooldownDefaultComponent>();
            systemState.RequireForUpdate<DashDurationDefaultComponent>();
            systemState.RequireForUpdate<InputDashComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();

            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float dashCooldownDefault = SystemAPI.GetSingleton<DashCooldownDefaultComponent>().Value;
            float dashDurationDefault = SystemAPI.GetSingleton<DashDurationDefaultComponent>().Value;
            uint inputDash = SystemAPI.GetSingleton<InputDashComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            systemState.Dependency = new DashJob { DashCooldownDefault = dashCooldownDefault , DeltaTime = SystemAPI.Time.DeltaTime , DashDurationDefault = dashDurationDefault , Ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , InputDashBit = inputDash , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct DashJob : IJobEntity
    {
        public float DashCooldownDefault;
        public float DashDurationDefault;
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public uint InputDashBit;
        public float TimerExpired;

        private void Execute(in BaseMoveSpeedComponent baseMoveSpeedComponent , ref DashCooldownComponent dashCooldownComponent , ref DashDurationComponent dashDurationComponent , in DashMultiplierComponent dashMultiplierComponent , EnabledRefRW<DashVisualTag> dashVisualTag , Entity entity , ref MoveSpeedComponent moveSpeedComponent , in PlayerInputComponent playerInputComponent)
        {
            bool isDashInputActive = (playerInputComponent.Value & InputDashBit) != 0;
            bool isCooldownReady = dashCooldownComponent.Value <= TimerExpired;

            int shouldDash = math.select(0 , 1 , isDashInputActive && isCooldownReady);
            for(int i = 0 ; i < shouldDash ; i++) { Ecb.AddComponent<DashPerformedTag>(entity.Index , entity); }

            dashDurationComponent.Value = math.select(math.max(TimerExpired , dashDurationComponent.Value - DeltaTime) , DashDurationDefault , isDashInputActive && isCooldownReady);
            dashCooldownComponent.Value = math.select(math.max(TimerExpired , dashCooldownComponent.Value - DeltaTime) , DashCooldownDefault , isDashInputActive && isCooldownReady);

            bool isDashing = dashDurationComponent.Value > TimerExpired;

            dashVisualTag.ValueRW = isDashing;
            moveSpeedComponent.Value = baseMoveSpeedComponent.Value * math.select(1 , dashMultiplierComponent.Value , isDashing);
        }
    }
}