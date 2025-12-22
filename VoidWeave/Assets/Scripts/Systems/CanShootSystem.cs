namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ShootingSystem))]
    public partial struct CanShootSystem : ISystem
    {
        private EntityQuery _playerQuery;
        private EntityQuery _inactiveShootersQuery;
        private EntityQuery _activeShootersQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _activeShootersQuery = SystemAPI.QueryBuilder().WithAll<CanShootTag>().Build();
            _inactiveShootersQuery = state.GetEntityQuery(new EntityQueryDesc { All = new ComponentType[] { typeof(CanShootTag) , typeof(CooldownComponent) } } , new EntityQueryDesc { All = new ComponentType[] { typeof(CanShootTag) } , None = new ComponentType[] { typeof(HasTargetTag) } });
            _playerQuery = SystemAPI.QueryBuilder().WithAll<PlayerTag>().WithNone<DeathTag>().Build();
            
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            int playerCount = _playerQuery.CalculateEntityCount();
            
            JobHandle grantHandle = new CanShootJob { ECB = ecb , PlayerCount = playerCount }.ScheduleParallel(state.Dependency);
            
            EntityQuery queryToUse = (playerCount == 0) ? _activeShootersQuery : _inactiveShootersQuery;

            state.Dependency = new CannotShootJob { ECB = ecb }.ScheduleParallel(queryToUse , grantHandle);
        }
    }

    [BurstCompile]
    [WithAll(typeof(HasTargetTag))]
    [WithNone(typeof(CooldownComponent) , typeof(CanShootTag))]
    public partial struct CanShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public int PlayerCount;

        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey)
        {
            for(int i = 0 ; i < math.select(0 , 1 , PlayerCount > 0) ; i++) ECB.AddComponent<CanShootTag>(sortKey , entity);
        }
    }

    [BurstCompile]
    public partial struct CannotShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey) { ECB.RemoveComponent<CanShootTag>(sortKey , entity); }
    }
}