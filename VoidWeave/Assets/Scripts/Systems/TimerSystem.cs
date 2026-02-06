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
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<TimerComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            systemState.Dependency = new TimerJob { DeltaTime = SystemAPI.Time.DeltaTime , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct TimerJob : IJobEntity
    {
        public float DeltaTime;
        public float TimerExpired;

        private void Execute(ref TimerComponent timerComponent)
        {
            timerComponent.Value -= DeltaTime;
            timerComponent.Value = math.max(TimerExpired , timerComponent.Value);
        }
    }
}