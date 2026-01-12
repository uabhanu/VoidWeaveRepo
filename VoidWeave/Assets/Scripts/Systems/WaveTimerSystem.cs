namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemySpawningSystem))]
    public partial struct WaveTimerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<WaveStateComponent>();
            systemState.RequireForUpdate<WaveTimerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new WaveTimerJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct WaveTimerJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(in WaveStateComponent waveStateComponent , ref WaveTimerComponent waveTimerComponent)
        {
            float decrement = math.select(0f , DeltaTime , waveStateComponent.State == 0);
            
            waveTimerComponent.Timer = math.max(0f , waveTimerComponent.Timer - decrement);
        }
    }
}