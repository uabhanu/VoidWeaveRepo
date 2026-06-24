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
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new DashJob { DashCooldownDefault = SystemAPI.GetSingleton<DashCooldownDefaultComponent>().Value , DeltaTime = SystemAPI.Time.DeltaTime , DashDurationDefault = SystemAPI.GetSingleton<DashDurationDefaultComponent>().Value , ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , InputDashBit = SystemAPI.GetSingleton<InputDashComponent>().Value , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct DashJob : IJobEntity
    {
        public float DashCooldownDefault;
        public float DashDurationDefault;
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        public uint InputDashBit;
        public float TimerExpired;

        private void Execute(in BaseMoveSpeedComponent baseMoveSpeedComponent , ref DashCooldownComponent dashCooldownComponent , ref DashDurationComponent dashDurationComponent , in DashMultiplierComponent dashMultiplierComponent , EnabledRefRW<DashVisualTag> dashVisualTag , Entity entity , ref MoveSpeedComponent moveSpeedComponent , in PlayerInputComponent playerInputComponent)
        {
            bool isDashInputActive = (playerInputComponent.Value & InputDashBit) != 0;
            bool isCooldownReady = dashCooldownComponent.Value <= TimerExpired;

            int shouldDash = math.select(0 , 1 , isDashInputActive && isCooldownReady);
            for(int i = 0 ; i < shouldDash ; i++) { ECB.SetComponentEnabled<DashPerformedTag>(entity.Index , entity , true); }

            dashDurationComponent.Value = math.select(math.max(TimerExpired , dashDurationComponent.Value - DeltaTime) , DashDurationDefault , isDashInputActive && isCooldownReady);
            dashCooldownComponent.Value = math.select(math.max(TimerExpired , dashCooldownComponent.Value - DeltaTime) , DashCooldownDefault , isDashInputActive && isCooldownReady);

            bool isDashing = dashDurationComponent.Value > TimerExpired;

            dashVisualTag.ValueRW = isDashing;
            moveSpeedComponent.Value = baseMoveSpeedComponent.Value * math.select(1 , dashMultiplierComponent.Value , isDashing);
        }
    }
}