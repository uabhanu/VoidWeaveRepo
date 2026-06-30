namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct CooldownSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }
        
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new CooldownJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct CooldownJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref CooldownComponent cooldownComponent) { cooldownComponent.Value -= DeltaTime; }
    }
}