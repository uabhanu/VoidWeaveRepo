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
        public void OnCreate(ref SystemState systemState)
        {
            _enemyTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent , EnemyTag>().WithNone<DeathTag>().Build();
            _playerTargetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent , PlayerTag>().WithNone<DeathTag>().Build();

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<TargetDefaultPositionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float targetDefaultPosition = SystemAPI.GetSingleton<TargetDefaultPositionComponent>().Value;

            NativeList<LocalToWorld> enemyPositionsNativeList = _enemyTargetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out JobHandle jobHandle1);
            NativeList<TeamComponent> enemyTeamComponentsNativeList = _enemyTargetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob , out JobHandle jobHandle2);

            NativeList<LocalToWorld> playerPositionsNativeList = _playerTargetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out JobHandle jobHandle3);
            NativeList<TeamComponent> playerTeamComponentsNativeList = _playerTargetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob , out JobHandle jobHandle4);

            JobHandle combinedDependencies = JobHandle.CombineDependencies(systemState.Dependency , jobHandle1 , jobHandle2);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , jobHandle3);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , jobHandle4);

            JobHandle enemyTheTargetJobHandle = new EnemyTheTargetJob { DoAction = doAction , ECBParallelWriter = ecbParallelWriter , NoAction = noAction , TargetDefaultPosition = targetDefaultPosition , TargetPositionsNativeList = enemyPositionsNativeList , TargetTeamComponentsNativeList = enemyTeamComponentsNativeList }.ScheduleParallel(combinedDependencies);
            JobHandle playerTheTargetJobHandle = new PlayerTheTargetJob { DoAction = doAction , ECBParallelWriter = ecbParallelWriter , NoAction = noAction , TargetPositionsNativeList = playerPositionsNativeList , TargetTeamComponentsNativeList = playerTeamComponentsNativeList }.ScheduleParallel(enemyTheTargetJobHandle);

            JobHandle disposeJobHandle1 = enemyPositionsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle2 = enemyTeamComponentsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle3 = playerPositionsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle4 = playerTeamComponentsNativeList.Dispose(playerTheTargetJobHandle);

            systemState.Dependency = JobHandle.CombineDependencies(disposeJobHandle1 , disposeJobHandle2 , JobHandle.CombineDependencies(disposeJobHandle3 , disposeJobHandle4));
        }
    }

    [BurstCompile]
    [WithAny(typeof(BeamTurretTag) , typeof(ScatterTurretTag) , typeof(StrikerTurretTag))]
    public partial struct EnemyTheTargetJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;
        public float TargetDefaultPosition;
        [ReadOnly] public NativeList<LocalToWorld> TargetPositionsNativeList;
        [ReadOnly] public NativeList<TeamComponent> TargetTeamComponentsNativeList;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in RangeComponent rangeComponent , ref TargetPositionComponent targetPositionComponent , in TeamComponent teamComponent)
        {
            var targetCurrentPosition = new float4(TargetDefaultPosition , TargetDefaultPosition , 0f , float.MaxValue);

            for(var i = 0 ; i < TargetPositionsNativeList.Length ; i++) targetCurrentPosition = math.select(targetCurrentPosition , new float4(TargetPositionsNativeList[i].Position , math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position)) , TargetTeamComponentsNativeList[i].Value != teamComponent.Value && math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position) < targetCurrentPosition.w);

            targetPositionComponent.Value = targetCurrentPosition.xyz;

            bool inRange = targetCurrentPosition.w <= rangeComponent.Value * rangeComponent.Value;

            for(var i = 0 ; i < math.select(NoAction , DoAction , inRange) ; i++) ECBParallelWriter.AddComponent<HasTargetTag>(entityIndexInQuery , entity);
            for(var i = 0 ; i < math.select(NoAction , DoAction , !inRange) ; i++) ECBParallelWriter.RemoveComponent<HasTargetTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct PlayerTheTargetJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;
        [ReadOnly] public NativeList<LocalToWorld> TargetPositionsNativeList;
        [ReadOnly] public NativeList<TeamComponent> TargetTeamComponentsNativeList;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in RangeComponent rangeComponent , ref TargetPositionComponent targetPositionComponent , in TeamComponent teamComponent)
        {
            var target = new float4(localToWorld.Position , float.MaxValue);

            for(var i = 0 ; i < TargetPositionsNativeList.Length ; i++) target = math.select(target , new float4(TargetPositionsNativeList[i].Position , math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position)) , TargetTeamComponentsNativeList[i].Value != teamComponent.Value && math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position) < target.w);

            targetPositionComponent.Value = target.xyz;

            bool inRange = target.w <= rangeComponent.Value * rangeComponent.Value;

            for(var i = 0 ; i < math.select(NoAction , DoAction , inRange) ; i++) ECBParallelWriter.AddComponent<HasTargetTag>(entityIndexInQuery , entity);
            for(var i = 0 ; i < math.select(NoAction , DoAction , !inRange) ; i++) ECBParallelWriter.RemoveComponent<HasTargetTag>(entityIndexInQuery , entity);
        }
    }
}