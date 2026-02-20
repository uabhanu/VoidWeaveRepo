namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ShootingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<SpreadHalfMultiplierComponent>();
            systemState.RequireForUpdate<SpreadZeroComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float spreadHalfMultiplier = SystemAPI.GetSingleton<SpreadHalfMultiplierComponent>().Value;
            float spreadZero = SystemAPI.GetSingleton<SpreadZeroComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            systemState.Dependency = new ShootJob { ECBParallelWriter = ecbParallelWriter , NoAction = noAction , SpreadHalfMultiplier = spreadHalfMultiplier , SpreadZero = spreadZero , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    public partial struct ShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;
        public float SpreadHalfMultiplier;
        public float SpreadZero;
        public float TimerExpired;

        private void Execute(in AttackRateComponent attackRateComponent , in BulletEntityComponent bulletEntityComponent , RefRW<CooldownComponent> cooldownComponent , in DamageComponent damageComponent , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in NozzleOffsetComponent nozzleOffsetComponent , in ProjectileCountComponent projectileCountComponent , in SpreadComponent spreadComponent)
        {
            bool isReady = cooldownComponent.ValueRO.Value <= TimerExpired;
            cooldownComponent.ValueRW.Value = math.select(cooldownComponent.ValueRO.Value , attackRateComponent.Value , isReady);

            int spawnCount = math.select(NoAction , projectileCountComponent.Value , isReady);

            // Get the actual world-space rotation of the turret nozzle
            quaternion turretRotation = localToWorld.Rotation;
            float3 spawnWorldPos = math.transform(localToWorld.Value , nozzleOffsetComponent.Value);

            for(var i = 0 ; i < spawnCount ; i++)
            {
                Entity newBullet = ECBParallelWriter.Instantiate(entityIndexInQuery , bulletEntityComponent.Entity);

                float spreadAngle = math.radians(spreadComponent.Value);
                float angleStep = math.select(SpreadZero , spreadAngle / math.max(1 , projectileCountComponent.Value - 1) , projectileCountComponent.Value > 1);

                // Using your established component for the half-offset
                float startOffset = spreadAngle * SpreadHalfMultiplier;

                // COMBINED ROTATION: Base turret rotation + the spread offset
                // We removed BulletRotationOffset because it is now handled by the BulletVisual child
                float currentAngleOffset = -startOffset + (angleStep * i);
                quaternion finalRotation = math.mul(turretRotation , quaternion.RotateZ(currentAngleOffset));

                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , new DamageComponent { Value = damageComponent.Value });
                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , LocalTransform.FromPositionRotation(spawnWorldPos , finalRotation));

                // DIRECTION FIX: Rotates World-Up by the turret's final calculated orientation
                // This forces the bullet to follow the nozzle direction precisely
                float3 direction = math.mul(finalRotation , math.up());
                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , new VelocityComponent { Value = direction.xy });
            }
        }
    }
}