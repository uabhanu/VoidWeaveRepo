namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct LifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            new LifetimeJob { DeltaTime = SystemAPI.Time.DeltaTime , DoAction = doAction , EntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct LifetimeJob : IJobEntity
    {
        public float DeltaTime;
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public int NoAction;
        public float TimerExpired;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , ref LifetimeComponent lifetimeComponent)
        {
            lifetimeComponent.Value -= DeltaTime;

            for(var i = 0 ; i < math.select(NoAction , DoAction , lifetimeComponent.Value <= TimerExpired) ; i++) EntityCommandBuffer.DestroyEntity(entityInQueryIndex , entity);
        }
    }
}