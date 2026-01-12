namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TimerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<TimerComponent>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new TimerJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct TimerJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref TimerComponent timerComponent)
        {
            timerComponent.Timer -= DeltaTime;
            timerComponent.Timer = math.max(0f , timerComponent.Timer);
        }
    }
}