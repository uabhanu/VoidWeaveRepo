namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<HealthValueForDeathComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float healthValueForDeath = SystemAPI.GetSingleton<HealthValueForDeathComponent>().Value;

            systemState.Dependency = new DamageJob { ECBParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , HealthValueForDeath = healthValueForDeath}.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(DamageEventComponent))]
    [WithNone(typeof(DeathTag))]
    public partial struct DamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public float HealthValueForDeath;

        private void Execute(in DamageEventComponent damageEventComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref CurrentHealthComponent currentHealthComponent)
        {
            currentHealthComponent.Value = math.max(HealthValueForDeath , currentHealthComponent.Value - damageEventComponent.Value);

            ECBParallelWriter.AddComponent<DamageTag>(entityIndexInQuery , entity);

            for(var i = 0 ; i < math.select(0 , 1 , currentHealthComponent.Value <= HealthValueForDeath) ; i++) ECBParallelWriter.AddComponent<DeathTag>(entityIndexInQuery , entity);

            ECBParallelWriter.RemoveComponent<DamageEventComponent>(entityIndexInQuery , entity);
        }
    }
}