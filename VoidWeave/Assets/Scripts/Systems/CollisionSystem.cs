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
            
            // Targets: Valid victims (Players & Enemies)
            _targetQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld , TeamComponent>().WithAny<EnemyTag , PlayerTag>().WithNone<DeathTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> targetEntitiesNativeArray = _targetQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<LocalToWorld> targetPositionsNativeArray = _targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<TeamComponent> targetTeamComponentsNativeArray = _targetQuery.ToComponentDataArray<TeamComponent>(Allocator.TempJob);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // PROJECTILES (Bullet -> Player/Enemy)
            // Kills Self (1) + Deals Damage
            JobHandle projectileJobHandle = new CollisionJob { ECB = ecb , HitRadiusSq = 0.5f * 0.5f , KillSelf = 1 , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<DamageComponent , LocalToWorld , ProjectileTag , TeamComponent>().WithNone<DeathTag>().Build() , state.Dependency);
            
            // Kills Self (0) + Deals Damage
            state.Dependency = new CollisionJob { ECB = ecb , HitRadiusSq = 0.5f * 0.5f , KillSelf = 0 , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<CanMeleeAttackTag , DamageComponent , EnemyTag , LocalToWorld , TeamComponent>().WithNone<DeathTag>().Build() , projectileJobHandle);

            targetEntitiesNativeArray.Dispose(state.Dependency);
            targetPositionsNativeArray.Dispose(state.Dependency);
            targetTeamComponentsNativeArray.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(DamageComponent) , typeof(LocalToWorld) , typeof(TeamComponent))]
    [WithNone(typeof(DeathTag))]
    public partial struct CollisionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float HitRadiusSq;
        public int KillSelf;

        [ReadOnly] public NativeArray<Entity> TargetEntitiesNativeArray;
        [ReadOnly] public NativeArray<LocalToWorld> TargetPositionsNativeArray;
        [ReadOnly] public NativeArray<TeamComponent> TargetTeamComponentsNativeArray;

        private void Execute(in DamageComponent damageComponent , Entity entity , in LocalToWorld localToWorld , [EntityIndexInQuery] int sortKey , in TeamComponent teamComponent)
        {
            for(int i = 0 ; i < TargetPositionsNativeArray.Length ; i++)
            {
                bool isHit = math.distancesq(localToWorld.Position , TargetPositionsNativeArray[i].Position) <= HitRadiusSq && teamComponent.ID != TargetTeamComponentsNativeArray[i].ID;

                // ADD DAMAGE EVENT
                for(int k = 0 ; k < math.select(0 , 1 , isHit) ; k++) { ECB.AddComponent(sortKey , TargetEntitiesNativeArray[i] , new DamageEventComponent { Damage = (int)damageComponent.Damage }); }

                // KILL SELF (Only if KillSelf is 1)
                for(int k = 0 ; k < math.select(0 , 1 , isHit && KillSelf == 1) ; k++) { ECB.AddComponent<DeathTag>(sortKey , entity); }
                
                // MELEE HIT TRIGGER (Enemies)
                // If we hit and we are an Enemy (KillSelf=0), add Tag to Self to trigger cooldown
                for(int k = 0 ; k < math.select(0 , 1 , isHit && KillSelf == 0) ; k++) { ECB.AddComponent<MeleeAttackEventTag>(sortKey , entity); }
            }
        }
    }
}