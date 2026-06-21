namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _targetQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            // Targets: Valid victims (Players & Enemies)
            _targetQuery = SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , LocalToWorld , TeamComponent>().WithAny<EnemyTag , PlayerTag>().WithNone<DeathTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            
            NativeArray<Entity> targetEntitiesNativeArray = _targetQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<LocalToWorld> targetPositionsNativeArray = _targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<CollisionRadiusComponent> targetRadiiNativeArray = _targetQuery.ToComponentDataArray<CollisionRadiusComponent>(Allocator.TempJob);
            NativeArray<TeamComponent> targetTeamComponentsNativeArray = _targetQuery.ToComponentDataArray<TeamComponent>(Allocator.TempJob);

            // PROJECTILES (Bullet -> Player/Enemy)
            // Kills Self (1) + Deals Entity
            JobHandle projectileJobHandle = new CollisionJob { ECB = ecb , KillSelf = true , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , DamageComponent , LocalToWorld , ProjectileTag , TeamComponent>().WithNone<DeathTag>().Build() , systemState.Dependency);

            // Kills Self (0) + Deals Entity
            systemState.Dependency = new CollisionJob { ECB = ecb , KillSelf = false , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<CanMeleeAttackTag , CollisionRadiusComponent , DamageComponent , EnemyTag , LocalToWorld , TeamComponent>().WithNone<DeathTag>().Build() , projectileJobHandle);

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
        public EntityCommandBuffer.ParallelWriter ECB;
        public bool KillSelf;

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
                for(var k = 0 ; k < math.select(0 , 1 , isHit) ; k++) ECB.AddComponent(entityIndexInQuery , TargetEntitiesNativeArray[i] , new DamageEventComponent { Value = (int)damageComponent.Value });

                // KILL SELF (Only if KillSelf is true)
                for(var k = 0 ; k < math.select(0 , 1 , isHit && KillSelf) ; k++) ECB.AddComponent<DeathTag>(entityIndexInQuery , entity);

                // MELEE HIT TRIGGER
                // If we hit and we are an Enemy (KillSelf=false), add Tag to Self to trigger cooldown
                for(var k = 0 ; k < math.select(0 , 1 , isHit && !KillSelf) ; k++) ECB.AddComponent<CanMeleeAttackTag>(entityIndexInQuery , entity);
            }
        }
    }
}