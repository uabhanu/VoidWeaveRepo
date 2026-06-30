namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct CollisionSystem : ISystem
    {
        private EntityQuery _enemyMeleeQuery;
        private EntityQuery _projectileQuery;
        private EntityQuery _targetQuery;
        
        public void OnCreate(ref SystemState systemState)
        {
            _enemyMeleeQuery = SystemAPI.QueryBuilder().WithAll<CanMeleeAttackTag , CollisionRadiusComponent , DamageComponent , EnemyTag , LocalToWorld , TeamComponent>().WithNone<DeathTag>().Build();
            _projectileQuery = SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , DamageComponent , LocalToWorld , ProjectileTag , TeamComponent>().WithNone<DeathTag>().Build();
            _targetQuery = SystemAPI.QueryBuilder().WithAll<CollisionRadiusComponent , LocalToWorld , TeamComponent>().WithAny<EnemyTag , PlayerTag>().WithNone<DeathTag>().Build();

            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState systemState)
        {
            // Native Collections MUST remain as local variables so they can be disposed
            NativeArray<Entity> targetEntitiesNativeArray = _targetQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<LocalToWorld> targetPositionsNativeArray = _targetQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<CollisionRadiusComponent> targetRadiiNativeArray = _targetQuery.ToComponentDataArray<CollisionRadiusComponent>(Allocator.TempJob);
            NativeArray<TeamComponent> targetTeamComponentsNativeArray = _targetQuery.ToComponentDataArray<TeamComponent>(Allocator.TempJob);

            // Inlined ECB and used cached queries!
            JobHandle projectileJobHandle = new CollisionJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , KillSelf = true , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(_projectileQuery , systemState.Dependency);

            systemState.Dependency = new CollisionJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , KillSelf = false , TargetEntitiesNativeArray = targetEntitiesNativeArray , TargetPositionsNativeArray = targetPositionsNativeArray , TargetRadiiNativeArray = targetRadiiNativeArray , TargetTeamComponentsNativeArray = targetTeamComponentsNativeArray }.ScheduleParallel(_enemyMeleeQuery , projectileJobHandle);

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

                // Enable Damage Event
                for(var k = 0 ; k < math.select(0 , 1 , isHit) ; k++)
                {
                    // Update the actual damage number in the entity's memory chunk
                    ECB.SetComponent(entityIndexInQuery , TargetEntitiesNativeArray[i] , new DamageEventComponent { Value = (int)damageComponent.Value });

                    // Flip the bit to 'true' so the DamageSystem and VFX System can process it
                    ECB.SetComponentEnabled<DamageEventComponent>(entityIndexInQuery , TargetEntitiesNativeArray[i] , true);
                }

                // Kill Self (Only if KillSelf is true)
                for(var k = 0 ; k < math.select(0 , 1 , isHit && KillSelf) ; k++) ECB.SetComponentEnabled<DeathTag>(entityIndexInQuery , entity , true);
                ;

                // Melee Hit Trigger
                // If we hit and we are an Enemy (KillSelf=false), add Tag to Self to trigger cooldown
                for(var k = 0 ; k < math.select(0 , 1 , isHit && !KillSelf) ; k++) ECB.SetComponentEnabled<CanMeleeAttackTag>(entityIndexInQuery , entity , true);
            }
        }
    }
}