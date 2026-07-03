namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct ShootingSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<TimerExpiredComponent>();
        }
        
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new ShootJob { ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    [WithAll(typeof(CanRangeAttackTag) , typeof(HasTargetTag) , typeof(RotationCompleteTag))]
    public partial struct ShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float TimerExpired;

        private void Execute(in AttackRateComponent attackRateComponent , in BulletEntityComponent bulletEntityComponent , RefRW<CooldownComponent> cooldownComponent , in DamageComponent damageComponent , Entity entity , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in ProjectileCountComponent projectileCountComponent , in ProjectileSpawnPointComponent projectileSpawnPointComponent , in SpreadComponent spreadComponent)
        {
            bool isReady = cooldownComponent.ValueRO.Value <= TimerExpired;
            cooldownComponent.ValueRW.Value = math.select(cooldownComponent.ValueRO.Value , attackRateComponent.Value , isReady);

            int spawnCount = math.select(0 , projectileCountComponent.Value , isReady);

            // Get the actual world-space rotation of the turret nozzle
            quaternion turretRotation = localToWorld.Rotation;
            float3 spawnWorldPos = math.transform(localToWorld.Value , projectileSpawnPointComponent.Value);

            for(var i = 0 ; i < spawnCount ; i++)
            {
                Entity newBullet = ECB.Instantiate(entityIndexInQuery , bulletEntityComponent.Entity);

                float spreadAngle = math.radians(spreadComponent.Value);
                float angleStep = math.select(0f , spreadAngle / math.max(1 , projectileCountComponent.Value - 1) , projectileCountComponent.Value > 1);

                // Using your established component for the half-offset
                float startOffset = spreadAngle * 0.5f;

                // Base turret rotation + the spread offset
                float currentAngleOffset = -startOffset + angleStep * i;
                quaternion finalRotation = math.mul(turretRotation , quaternion.RotateZ(currentAngleOffset));

                ECB.SetComponent(entityIndexInQuery , newBullet , new DamageComponent { Value = damageComponent.Value });
                ECB.SetComponent(entityIndexInQuery , newBullet , LocalTransform.FromPositionRotation(spawnWorldPos , finalRotation));

                // Rotates World-Up by the turret's final calculated orientation
                // This forces the bullet to follow the nozzle direction precisely
                float3 direction = math.mul(finalRotation , math.up());
                ECB.SetComponent(entityIndexInQuery , newBullet , new VelocityComponent { Value = direction.xy });
            }

            for(int i = 0 ; i < math.select(0 , 1 , isReady) ; i++) { ECB.SetComponentEnabled<ProjectileFiredEventTag>(entityIndexInQuery , entity , true); }
        }
    }
}