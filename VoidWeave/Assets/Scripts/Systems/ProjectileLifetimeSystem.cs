namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ProjectileLifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DoActionComponent>();
            state.RequireForUpdate<NoActionComponent>();
            state.RequireForUpdate<ProjectileTag>();
            state.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            new ProjectileLifetimeJob { DeltaTime = SystemAPI.Time.DeltaTime , DoAction = doAction , EntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ProjectileLifetimeJob : IJobEntity
    {
        public float DeltaTime;
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public int NoAction;
        public float TimerExpired;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , ref ProjectileLifetimeComponent projectileLifetimeComponent)
        {
            projectileLifetimeComponent.Value -= DeltaTime;
            
            for(int i = 0 ; i < math.select(NoAction , DoAction , projectileLifetimeComponent.Value <= TimerExpired) ; i++) { EntityCommandBuffer.DestroyEntity(entityInQueryIndex , entity); }
        }
    }
}