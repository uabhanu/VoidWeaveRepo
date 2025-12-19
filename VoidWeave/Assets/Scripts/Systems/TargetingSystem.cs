namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct TargetingSystem : ISystem
    {
        private EntityQuery _enemyTargetQuery;
        private EntityQuery _playerTargetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyTag>();
            state.RequireForUpdate<PlayerTag>();
            
            _enemyTargetQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag , LocalToWorld>().Build();
            _playerTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , PlayerTag>().Build();
            
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
    [WithAll(typeof(ProjectileTag))]
    [WithNone(typeof(EnemyTag))]
    public partial struct EnemyTheTargetJob : IJobEntity
    {
        public float3 TargetPosition;

        private void Execute(ref TargetPositionComponent targetPositionComponent) { targetPositionComponent.Position = TargetPosition; }
    }
    
    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct PlayerTheTargetJob : IJobEntity
    {
        public float3 TargetPosition;

        private void Execute(ref TargetPositionComponent targetPositionComponent) { targetPositionComponent.Position = TargetPosition; }
    }
}