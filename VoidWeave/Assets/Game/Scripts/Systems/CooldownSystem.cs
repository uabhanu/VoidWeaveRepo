namespace Game.Scripts.Systems
{
    using Game.Scripts.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new CooldownJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct CooldownJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref CooldownComponent cooldownComponent) { cooldownComponent.Value -= DeltaTime; }
    }
}