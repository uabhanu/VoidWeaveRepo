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
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _targetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _targetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent>().WithAny<EnemyTag , PlayerTag>().WithNone<DeathTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> targetEntitiesNativeArray = _targetQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<LocalToWorld> targetPositionsNativeArray = _targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<TeamComponent> targetTeamComponentsNativeArray = _targetQuery.ToComponentDataArray<TeamComponent>(Allocator.TempJob);

            // 1. Projectiles: Kill Self + Kill Target (KillTarget = 1)
            JobHandle projectileJobHandle = new CollisionJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , HitRadiusSq = 0.5f * 0.5f , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray , KillTarget = 1 }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent , ProjectileTag>().WithNone<DeathTag>().Build() , state.Dependency);

            // 2. Players: Kill Self + Spare Target (KillTarget = 0)
            state.Dependency = new CollisionJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() , HitRadiusSq = 0.5f * 0.5f , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray , KillTarget = 0 }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent , PlayerTag>().WithNone<DeathTag>().Build() , projectileJobHandle);

            targetEntitiesNativeArray.Dispose(state.Dependency);
            targetPositionsNativeArray.Dispose(state.Dependency);
            targetTeamComponentsNativeArray.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAny(typeof(PlayerTag) , typeof(ProjectileTag))]
    [WithNone(typeof(DeathTag))]
    public partial struct CollisionJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> TargetEntitiesNativeArray;
        [ReadOnly] public NativeArray<LocalToWorld> TargetPositionsNativeArray;
        [ReadOnly] public NativeArray<TeamComponent> TargetTeamComponentsNativeArray;

        public EntityCommandBuffer.ParallelWriter ECB;
        public float HitRadiusSq;
        public int KillTarget;

        private void Execute(Entity projectileEntity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in TeamComponent projectileTeam)
        {
            for(int i = 0 ; i < TargetPositionsNativeArray.Length ; i++)
            {
                for(int k = 0 ; k < math.select(0 , 1 , math.step(math.distancesq(localToWorld.Position , TargetPositionsNativeArray[i].Position) , HitRadiusSq) > 0.5f && projectileTeam.ID != TargetTeamComponentsNativeArray[i].ID) ; k++)
                {
                    ECB.AddComponent<DeathTag>(entityIndexInQuery , projectileEntity);
                    
                    for(int m = 0 ; m < math.select(0 , 1 , KillTarget == 1) ; m++) { ECB.AddComponent<DeathTag>(entityIndexInQuery , TargetEntitiesNativeArray[i]); }
                }
            }
        }
    }
}