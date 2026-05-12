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
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _targetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<CollisionActiveComponent>();
            systemState.RequireForUpdate<CollisionNoneComponent>();

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

            int collisionActiveValue = SystemAPI.GetSingleton<CollisionActiveComponent>().Value;
            int collisionNoneValue = SystemAPI.GetSingleton<CollisionNoneComponent>().Value;

            EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            // PROJECTILES (Bullet -> Player/Enemy)
            // Kills Self (1) + Deals Entity
            JobHandle projectileJobHandle = new CollisionJob { CollisionActiveValue = collisionActiveValue , CollisionNoneValue = collisionNoneValue , ECB = ecb , KillSelf = collisionActiveValue , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , DamageComponent , LocalToWorld , ProjectileTag , TeamComponent>().WithNone<DeathTag>().Build() , systemState.Dependency);

            // Kills Self (0) + Deals Entity
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
            for(var i = 0 ; i < TargetPositionsNativeArray.Length ; i++)
            {
                float combinedRadius = collisionRadiusComponent.Value + TargetRadiiNativeArray[i].Value;
                float hitRadiusSq = combinedRadius * combinedRadius;
                bool isHit = math.distancesq(localToWorld.Position , TargetPositionsNativeArray[i].Position) <= hitRadiusSq && teamComponent.Value != TargetTeamComponentsNativeArray[i].Value;

                // ADD DAMAGE EVENT
                for(var k = 0 ; k < math.select(CollisionNoneValue , CollisionActiveValue , isHit) ; k++) ECB.AddComponent(entityIndexInQuery , TargetEntitiesNativeArray[i] , new DamageEventComponent { Value = (int)damageComponent.Value });

                // KILL SELF (Only if KillSelf is 1)
                for(var k = 0 ; k < math.select(CollisionNoneValue , CollisionActiveValue , isHit && KillSelf == CollisionActiveValue) ; k++) ECB.AddComponent<DeathTag>(entityIndexInQuery , entity);

                // MELEE HIT TRIGGER
                // If we hit and we are an Enemy (KillSelf=0), add Tag to Self to trigger cooldown
                for(var k = 0 ; k < math.select(CollisionNoneValue , CollisionActiveValue , isHit && KillSelf == CollisionNoneValue) ; k++) ECB.AddComponent<CanMeleeAttackTag>(entityIndexInQuery , entity);
            }
        }
    }
}