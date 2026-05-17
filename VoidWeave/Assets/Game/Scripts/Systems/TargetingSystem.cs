namespace Game.Scripts.Systems
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
            float3 targetDefaultPosition = SystemAPI.GetSingleton<TargetDefaultPositionComponent>().Value;

            NativeList<Entity> enemyEntitiesNativeList = _enemyTargetQuery.ToEntityListAsync(Allocator.TempJob , out JobHandle jobHandle1);
            NativeList<LocalToWorld> enemyPositionsNativeList = _enemyTargetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out JobHandle jobHandle2);
            NativeList<TeamComponent> enemyTeamComponentsNativeList = _enemyTargetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob , out JobHandle jobHandle3);

            NativeList<Entity> playerEntitiesNativeList = _playerTargetQuery.ToEntityListAsync(Allocator.TempJob , out JobHandle jobHandle4);
            NativeList<LocalToWorld> playerPositionsNativeList = _playerTargetQuery.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob , out JobHandle jobHandle5);
            NativeList<TeamComponent> playerTeamComponentsNativeList = _playerTargetQuery.ToComponentDataListAsync<TeamComponent>(Allocator.TempJob , out JobHandle jobHandle6);

            JobHandle combinedDependencies = JobHandle.CombineDependencies(systemState.Dependency , jobHandle1 , jobHandle2);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , jobHandle3);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , jobHandle4);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , jobHandle5);
            combinedDependencies = JobHandle.CombineDependencies(combinedDependencies , jobHandle6);

            JobHandle enemyTheTargetJobHandle = new EnemyTheTargetJob { DoAction = doAction , ECBParallelWriter = ecbParallelWriter , NoAction = noAction , TargetEntitiesNativeList = enemyEntitiesNativeList , TargetDefaultPosition = targetDefaultPosition , TargetPositionsNativeList = enemyPositionsNativeList , TargetTeamComponentsNativeList = enemyTeamComponentsNativeList }.ScheduleParallel(combinedDependencies);
            JobHandle playerTheTargetJobHandle = new PlayerTheTargetJob { DoAction = doAction , ECBParallelWriter = ecbParallelWriter , NoAction = noAction , TargetEntitiesNativeList = playerEntitiesNativeList , TargetDefaultPosition = targetDefaultPosition , TargetPositionsNativeList = playerPositionsNativeList , TargetTeamComponentsNativeList = playerTeamComponentsNativeList }.ScheduleParallel(enemyTheTargetJobHandle);

            JobHandle disposeJobHandle1 = enemyEntitiesNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle2 = enemyPositionsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle3 = enemyTeamComponentsNativeList.Dispose(playerTheTargetJobHandle);

            JobHandle disposeJobHandle4 = playerEntitiesNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle5 = playerPositionsNativeList.Dispose(playerTheTargetJobHandle);
            JobHandle disposeJobHandle6 = playerTeamComponentsNativeList.Dispose(playerTheTargetJobHandle);

            JobHandle disposeGroup1 = JobHandle.CombineDependencies(disposeJobHandle1 , disposeJobHandle2 , disposeJobHandle3);
            JobHandle disposeGroup2 = JobHandle.CombineDependencies(disposeJobHandle4 , disposeJobHandle5 , disposeJobHandle6);
            systemState.Dependency = JobHandle.CombineDependencies(disposeGroup1 , disposeGroup2);
        }
    }

    [BurstCompile]
    [WithAny(typeof(BeamTurretTag) , typeof(ScatterTurretTag) , typeof(StrikerTurretTag))]
    public partial struct EnemyTheTargetJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;
        public float3 TargetDefaultPosition;

        [ReadOnly] public NativeList<Entity> TargetEntitiesNativeList;
        [ReadOnly] public NativeList<LocalToWorld> TargetPositionsNativeList;
        [ReadOnly] public NativeList<TeamComponent> TargetTeamComponentsNativeList;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in RangeComponent rangeComponent , ref TargetEntityComponent targetEntityComponent , ref TargetPositionComponent targetPositionComponent , in TeamComponent teamComponent)
        {
            var closestTargetPosition = new float4(TargetDefaultPosition , float.MaxValue);
            var selectedEntity = Entity.Null;

            for(var i = 0 ; i < TargetPositionsNativeList.Length ; i++)
            {
                float dSq = math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position);
                bool isSelectedTarget = TargetTeamComponentsNativeList[i].Value != teamComponent.Value && dSq < closestTargetPosition.w;
                closestTargetPosition = math.select(closestTargetPosition , new float4(TargetPositionsNativeList[i].Position , math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position)) , TargetTeamComponentsNativeList[i].Value != teamComponent.Value && math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position) < closestTargetPosition.w);
                selectedEntity = isSelectedTarget ? TargetEntitiesNativeList[i] : selectedEntity;
            }

            bool isSelectedTargetStillExists = false;
            var currentTargetPosition = new float4(TargetDefaultPosition , float.MaxValue);

            for(var i = 0 ; i < TargetEntitiesNativeList.Length ; i++)
            {
                bool isCurrent = targetEntityComponent.Entity == TargetEntitiesNativeList[i];
                float dSq = math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position);
                bool inRangeCurrent = dSq <= rangeComponent.Value * rangeComponent.Value;
                isSelectedTargetStillExists = isCurrent && inRangeCurrent || isSelectedTargetStillExists;
                currentTargetPosition = math.select(currentTargetPosition , new float4(TargetPositionsNativeList[i].Position , dSq) , isCurrent && inRangeCurrent);
            }

            closestTargetPosition = math.select(closestTargetPosition , currentTargetPosition , isSelectedTargetStillExists);
            selectedEntity = isSelectedTargetStillExists ? targetEntityComponent.Entity : selectedEntity;

            targetPositionComponent.Value = closestTargetPosition.xyz;
            targetEntityComponent.Entity = selectedEntity;

            bool inRangeFinal = closestTargetPosition.w <= rangeComponent.Value * rangeComponent.Value && selectedEntity != Entity.Null;

            for(var i = 0 ; i < math.select(NoAction , DoAction , inRangeFinal) ; i++) ECBParallelWriter.AddComponent<HasTargetTag>(entityIndexInQuery , entity);
            for(var i = 0 ; i < math.select(NoAction , DoAction , !inRangeFinal) ; i++) ECBParallelWriter.RemoveComponent<HasTargetTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct PlayerTheTargetJob : IJobEntity
    {
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;
        public float3 TargetDefaultPosition;

        [ReadOnly] public NativeList<Entity> TargetEntitiesNativeList;
        [ReadOnly] public NativeList<LocalToWorld> TargetPositionsNativeList;
        [ReadOnly] public NativeList<TeamComponent> TargetTeamComponentsNativeList;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in RangeComponent rangeComponent , ref TargetEntityComponent targetEntityComponent , ref TargetPositionComponent targetPositionComponent , in TeamComponent teamComponent)
        {
            var closestTargetPosition = new float4(TargetDefaultPosition , float.MaxValue);
            var selectedEntity = Entity.Null;

            for(var i = 0 ; i < TargetPositionsNativeList.Length ; i++)
            {
                float dSq = math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position);
                bool isSelectedTarget = TargetTeamComponentsNativeList[i].Value != teamComponent.Value && dSq < closestTargetPosition.w;
                closestTargetPosition = math.select(closestTargetPosition , new float4(TargetPositionsNativeList[i].Position , dSq) , isSelectedTarget);
                selectedEntity = isSelectedTarget ? TargetEntitiesNativeList[i] : selectedEntity;
            }

            bool isSelectedTargetStillExists = false;
            var currentTargetPosition = new float4(TargetDefaultPosition , float.MaxValue);

            for(var i = 0 ; i < TargetEntitiesNativeList.Length ; i++)
            {
                bool isCurrent = targetEntityComponent.Entity == TargetEntitiesNativeList[i];
                float dSq = math.distancesq(localToWorld.Position , TargetPositionsNativeList[i].Position);
                bool inRangeCurrent = dSq <= rangeComponent.Value * rangeComponent.Value;
                isSelectedTargetStillExists = isCurrent && inRangeCurrent || isSelectedTargetStillExists;
                currentTargetPosition = math.select(currentTargetPosition , new float4(TargetPositionsNativeList[i].Position , dSq) , isCurrent && inRangeCurrent);
            }

            closestTargetPosition = math.select(closestTargetPosition , currentTargetPosition , isSelectedTargetStillExists);
            selectedEntity = isSelectedTargetStillExists ? targetEntityComponent.Entity : selectedEntity;

            targetPositionComponent.Value = closestTargetPosition.xyz;
            targetEntityComponent.Entity = selectedEntity;

            bool inRangeFinal = closestTargetPosition.w <= rangeComponent.Value * rangeComponent.Value && selectedEntity != Entity.Null;

            for(var i = 0 ; i < math.select(NoAction , DoAction , inRangeFinal) ; i++) ECBParallelWriter.AddComponent<HasTargetTag>(entityIndexInQuery , entity);
            for(var i = 0 ; i < math.select(NoAction , DoAction , !inRangeFinal) ; i++) ECBParallelWriter.RemoveComponent<HasTargetTag>(entityIndexInQuery , entity);
        }
    }
}