namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct GameOverSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<DeathTag , PlayerTag>().WithNone<RestartTag>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecbCleanup = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            EntityCommandBuffer ecbRestart = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            new CleanupJob { ECBParallelWriter = ecbCleanup.AsParallelWriter() }.ScheduleParallel(SystemAPI.QueryBuilder().WithAny<EnemyTag , ProjectileTag , ScatterTurretTag , StrikerTurretTag>().Build());

            Entity restartEntity = ecbRestart.CreateEntity();
            ecbRestart.AddComponent<RestartTag>(restartEntity);
        }
    }

    [BurstCompile]
    public partial struct CleanupJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery) { ECBParallelWriter.DestroyEntity(entityIndexInQuery , entity); }
    }
}