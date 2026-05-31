namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(LifetimeSystem))]
    public partial struct EntityPulseSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;

            new PulseJob { DoAction = doAction , EntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , NoAction = noAction}.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(LootTag))]
    [WithNone(typeof(PulseTag))]
    public partial struct PulseJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public int NoAction;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityInQueryIndex , in LifetimeComponent lifetimeComponent , in TimeBeforeEntityPulseComponent timeBeforePulse)
        {
            for(var i = 0 ; i < math.select(NoAction , DoAction , lifetimeComponent.Value <= timeBeforePulse.Value) ; i++) EntityCommandBuffer.AddComponent(entityInQueryIndex , entity , new PulseTag());
        }
    }
}