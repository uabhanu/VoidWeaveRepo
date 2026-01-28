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
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<CollisionActiveValueComponent>();
            systemState.RequireForUpdate<CollisionNoneValueComponent>();

            // Targets: Valid victims (Players & Enemies)
            _targetQuery = SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , LocalToWorld , TeamComponent>().WithAny<EnemyTag , PlayerTag>().WithNone<DeathTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            NativeArray<Entity> targetEntitiesNativeArray = _targetQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<LocalToWorld> targetPositionsNativeArray = _targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<CollisionRadiusComponent> targetRadiiNativeArray = _targetQuery.ToComponentDataArray<CollisionRadiusComponent>(Allocator.TempJob);
            NativeArray<TeamComponent> targetTeamComponentsNativeArray = _targetQuery.ToComponentDataArray<TeamComponent>(Allocator.TempJob);

            int collisionActiveValue = SystemAPI.GetSingleton<CollisionActiveValueComponent>().CollisionActiveValue;
            int collisionNoneValue = SystemAPI.GetSingleton<CollisionNoneValueComponent>().CollisionNoneValue;

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            // PROJECTILES (Bullet -> Player/Enemy)
            // Kills Self (1) + Deals Damage
            JobHandle projectileJobHandle = new CollisionJob { CollisionActiveValue = collisionActiveValue , CollisionNoneValue = collisionNoneValue , ECB = ecb , KillSelf = collisionActiveValue , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , DamageComponent , LocalToWorld , ProjectileTag , TeamComponent>().WithNone<DeathTag>().Build() , systemState.Dependency);

            // Kills Self (0) + Deals Damage
            systemState.Dependency = new CollisionJob { CollisionActiveValue = collisionActiveValue , CollisionNoneValue = collisionNoneValue , ECB = ecb , KillSelf = collisionNoneValue , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<CanMeleeAttackTag , CollisionRadiusComponent , DamageComponent , EnemyTag , LocalToWorld , TeamComponent>().WithNone<DeathTag>().Build() , projectileJobHandle);

            targetEntitiesNativeArray.Dispose(systemState.Dependency);
            targetPositionsNativeArray.Dispose(systemState.Dependency);
            targetRadiiNativeArray.Dispose(systemState.Dependency);
            targetTeamComponentsNativeArray.Dispose(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(DamageComponent) , typeof(LocalToWorld) , typeof(TeamComponent))]
    [WithNone(typeof(DeathTag))]
    public partial struct CollisionJob : IJobEntity
    {
        public int CollisionActiveValue;
        public int CollisionNoneValue;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int KillSelf;

        [ReadOnly] public NativeArray<Entity> TargetEntitiesNativeArray;
        [ReadOnly] public NativeArray<LocalToWorld> TargetPositionsNativeArray;
        [ReadOnly] public NativeArray<CollisionRadiusComponent> TargetRadiiNativeArray;
        [ReadOnly] public NativeArray<TeamComponent> TargetTeamComponentsNativeArray;

        private void Execute(in CollisionRadiusComponent collisionRadiusComponent , in DamageComponent damageComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in TeamComponent teamComponent)
        {
            for(int i = 0 ; i < TargetPositionsNativeArray.Length ; i++)
            {
                float combinedRadius = collisionRadiusComponent.Radius + TargetRadiiNativeArray[i].Radius;
                float hitRadiusSq = combinedRadius * combinedRadius;
                bool isHit = math.distancesq(localToWorld.Position , TargetPositionsNativeArray[i].Position) <= hitRadiusSq && teamComponent.ID != TargetTeamComponentsNativeArray[i].ID;

                // ADD DAMAGE EVENT
                for(int k = 0 ; k < math.select(CollisionNoneValue , CollisionActiveValue , isHit) ; k++) { ECB.AddComponent(entityIndexInQuery , TargetEntitiesNativeArray[i] , new DamageEventComponent { Damage = (int)damageComponent.Damage }); }

                // KILL SELF (Only if KillSelf is 1)
                for(int k = 0 ; k < math.select(CollisionNoneValue , CollisionActiveValue , isHit && KillSelf == CollisionActiveValue) ; k++) { ECB.AddComponent<DeathTag>(entityIndexInQuery , entity); }

                // MELEE HIT TRIGGER
                // If we hit and we are an Enemy (KillSelf=0), add Tag to Self to trigger cooldown
                for(int k = 0 ; k < math.select(CollisionNoneValue , CollisionActiveValue , isHit && KillSelf == CollisionNoneValue) ; k++) { ECB.AddComponent<CanMeleeAttackTag>(entityIndexInQuery , entity); }
            }
        }
    }
}