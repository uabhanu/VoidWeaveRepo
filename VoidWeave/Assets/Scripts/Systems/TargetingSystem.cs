namespace Systems
{
    using Gameplay;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TargetingSystem : ISystem
    {
        private EntityQuery _enemyTargetQuery;
        private EntityQuery _playerTargetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<SeekerTag>();
            state.RequireForUpdate<TargetTag>();
            state.RequireForUpdate<TurretTargetTag>();
            
            _enemyTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TargetTag , TurretTargetTag>().Build();
            _playerTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , PlayerTag , TargetTag>().Build();
            
            state.RequireForUpdate(_enemyTargetQuery);
            state.RequireForUpdate(_playerTargetQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float3 playerPos = _playerTargetQuery.GetSingleton<LocalToWorld>().Position;
            NativeArray<LocalToWorld> enemyTransforms = _enemyTargetQuery.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            float3 enemyPos = enemyTransforms[0].Position;
            enemyTransforms.Dispose();

            new EnemyTheTargetJob { TargetPosition = enemyPos }.ScheduleParallel();
            new PlayerTheTargetJob { TargetPosition = playerPos }.ScheduleParallel();
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(ProjectileTag) , typeof(SeekerTag))]
    [WithNone(typeof(TurretTargetTag))]
    public partial struct EnemyTheTargetJob : IJobEntity
    {
        public float3 TargetPosition;

        private void Execute(ref TargetPositionComponent targetPositionComponent) { targetPositionComponent.TargetPosition = TargetPosition; }
    }
    
    [BurstCompile]
    [WithAll(typeof(SeekerTag) , typeof(TurretTargetTag))]
    public partial struct PlayerTheTargetJob : IJobEntity
    {
        public float3 TargetPosition;

        private void Execute(ref TargetPositionComponent targetPositionComponent) { targetPositionComponent.TargetPosition = TargetPosition; }
    }
}