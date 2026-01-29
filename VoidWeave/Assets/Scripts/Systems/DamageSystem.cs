namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<HealthValueForDeathComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().DoActionValue;
            float healthValueForDeath = SystemAPI.GetSingleton<HealthValueForDeathComponent>().HealthValueForDeath;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().NoActionValue;
            
            systemState.Dependency = new DamageJob { HealthValueForDeath = healthValueForDeath , DoAction = doAction , ECBParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , NoAction = noAction }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(DamageEventComponent))]
    [WithNone(typeof(DeathTag))]
    public partial struct DamageJob : IJobEntity
    {
        public int DoAction;
        public float HealthValueForDeath;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;

        private void Execute(in DamageEventComponent damageEventComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref CurrentHealthComponent currentHealthComponent)
        {
            currentHealthComponent.CurrentHealth -= damageEventComponent.Damage;
            
            for(int i = 0 ; i < math.select(NoAction , DoAction , currentHealthComponent.CurrentHealth <= HealthValueForDeath) ; i++) { ECBParallelWriter.AddComponent<DeathTag>(entityIndexInQuery , entity); }
            
            ECBParallelWriter.RemoveComponent<DamageEventComponent>(entityIndexInQuery , entity);
        }
    }
}