namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ShootingSystem))]
    public partial struct TargetingSystem : ISystem
    {
        private EntityQuery _enemyTargetQuery;
        private EntityQuery _playerTargetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _enemyTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent , EnemyTag>().WithNone<DeathTag>().Build();
            _playerTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , PlayerTag>().WithNone<DeathTag>().Build();

            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeList<LocalToWorld> enemyPositionsNativeList = _enemyTargetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out var h1);
            NativeList<TeamComponent> enemyTeamComponentsNativeList = _enemyTargetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob , out var h2);
            NativeList<LocalToWorld> playerPositionsNativeList = _playerTargetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out var h3);

            JobHandle combinedDependencies = JobHandle.CombineDependencies(state.Dependency , h1 , h2);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , h3);
            
            JobHandle enemyTheTargetJobHandle = new EnemyTheTargetJob { TargetPositionsNativeList = enemyPositionsNativeList , TargetTeamComponentsNativeList = enemyTeamComponentsNativeList }.ScheduleParallel(combinedDependencies);
            JobHandle playerTheTargetJobHandle = new PlayerTheTargetJob { TargetPositionsNativeList = playerPositionsNativeList }.ScheduleParallel(enemyTheTargetJobHandle);
            
            JobHandle disposeH1 = enemyPositionsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeH2 = enemyTeamComponentsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeH3 = playerPositionsNativeList.Dispose(playerTheTargetJobHandle);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            state.Dependency = new TargetRangeJob { ECB = ecb }.ScheduleParallel(JobHandle.CombineDependencies(playerTheTargetJobHandle , JobHandle.CombineDependencies(disposeH1 , disposeH2 , disposeH3)));
        }
    }

    [BurstCompile]
    [WithAny(typeof(ScatterTurretTag) , typeof(StrikerTurretTag))]
    public partial struct EnemyTheTargetJob : IJobEntity
    {
        [ReadOnly] public NativeList<LocalToWorld> TargetPositionsNativeList;
        [ReadOnly] public NativeList<TeamComponent> TargetTeamComponentsNativeList;

        private void Execute(in LocalToWorld localToWorld , ref TargetPositionComponent targetPositionComponent , in TeamComponent teamComponent)
        {
            float4 bestTarget = new float4(100000f , 100000f , 0f , float.MaxValue);

            for(int i = 0 ; i < TargetPositionsNativeList.Length ; i++) { bestTarget = math.select(bestTarget , new float4(TargetPositionsNativeList[i].Position , math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position)) , (TargetTeamComponentsNativeList[i].ID != teamComponent.ID) && (math.lengthsq(TargetPositionsNativeList[i].Position) > 0.001f) && (math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position) < bestTarget.w)); }

            targetPositionComponent.Position = bestTarget.xyz;
        }
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct PlayerTheTargetJob : IJobEntity
    {
        [ReadOnly] public NativeList<LocalToWorld> TargetPositionsNativeList;
        
        private void Execute(in LocalToWorld localToWorld , ref TargetPositionComponent targetPositionComponent)
        {
            float4 bestTarget = new float4(localToWorld.Position , float.MaxValue);
            
            for(int i = 0 ; i < TargetPositionsNativeList.Length ; i++) { bestTarget = math.select(bestTarget , new float4(TargetPositionsNativeList[i].Position , math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position)) , (math.lengthsq(TargetPositionsNativeList[i].Position) > 0.001f) && (math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position) < bestTarget.w)); }

            targetPositionComponent.Position = bestTarget.xyz;
        }
    }

    [BurstCompile]
    public partial struct TargetRangeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(Entity entity , in LocalToWorld localToWorld , [EntityIndexInQuery] int sortKey , in TargetPositionComponent targetPositionComponent , in TurretRangeComponent turretRangeComponent)
        {
            bool inRange = math.distancesq(localToWorld.Position , targetPositionComponent.Position) <= turretRangeComponent.Range * turretRangeComponent.Range && math.lengthsq(targetPositionComponent.Position) > 0.001f;

            for(int i = 0 ; i < math.select(0 , 1 , inRange) ; i++) ECB.AddComponent<HasTargetTag>(sortKey , entity);

            for(int i = 0 ; i < math.select(0 , 1 , !inRange) ; i++) ECB.RemoveComponent<HasTargetTag>(sortKey , entity);
        }
    }
}