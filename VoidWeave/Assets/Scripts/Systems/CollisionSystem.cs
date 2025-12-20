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
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _targetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _targetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent>().WithAny<EnemyTag , PlayerTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> targetEntitiesNativeArray = _targetQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<LocalToWorld> targetPositionsNativeArray = _targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<TeamComponent> targetTeamComponentsNativeArray = _targetQuery.ToComponentDataArray<TeamComponent>(Allocator.TempJob);
            
            state.Dependency = new CollisionJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , HitRadiusSq = 0.5f * 0.5f , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(state.Dependency);

            targetEntitiesNativeArray.Dispose(state.Dependency);
            targetPositionsNativeArray.Dispose(state.Dependency);
            targetTeamComponentsNativeArray.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileTag))]
    public partial struct CollisionJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> TargetEntitiesNativeArray;
        [ReadOnly] public NativeArray<LocalToWorld> TargetPositionsNativeArray;
        [ReadOnly] public NativeArray<TeamComponent> TargetTeamComponentsNativeArray;

        public EntityCommandBuffer.ParallelWriter ECB;
        public float HitRadiusSq;
        
        private void Execute(Entity projectileEntity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in TeamComponent projectileTeam)
        {
            for(int i = 0 ; i < TargetPositionsNativeArray.Length ; i++)
            {
                for(int k = 0 ; k < math.select(0 , 1 , math.step(math.distancesq(localToWorld.Position , TargetPositionsNativeArray[i].Position) , HitRadiusSq) > 0.5f && projectileTeam.ID != TargetTeamComponentsNativeArray[i].ID) ; k++)
                {
                    ECB.AddComponent<DeathTag>(entityIndexInQuery , projectileEntity);
                    ECB.AddComponent<DeathTag>(entityIndexInQuery , TargetEntitiesNativeArray[i]);
                }
            }
        }
    }
}