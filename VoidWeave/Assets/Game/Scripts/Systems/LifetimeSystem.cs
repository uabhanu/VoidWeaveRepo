namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct LifetimeSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<TimerExpiredComponent>();
        }
        
        public void OnUpdate(ref SystemState systemState) { new LifetimeJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(); }
    }
    
    [BurstCompile]
    public partial struct LifetimeJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        public float TimerExpired;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , ref LifetimeComponent lifetimeComponent)
        {
            lifetimeComponent.Value -= DeltaTime;

            for(var i = 0 ; i < math.select(0 , 1 , lifetimeComponent.Value <= TimerExpired) ; i++) ECB.DestroyEntity(entityInQueryIndex , entity);
        }
    }
}