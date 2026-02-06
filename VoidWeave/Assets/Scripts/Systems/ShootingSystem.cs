namespace Systems
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

            systemState.RequireForUpdate<BulletRotationOffsetComponent>();
            systemState.RequireForUpdate<MinProjectileCountComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<SpreadHalfMultiplierComponent>();
            systemState.RequireForUpdate<SpreadZeroComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            float bulletRotationOffset = SystemAPI.GetSingleton<BulletRotationOffsetComponent>().Value;
            int minProjectileCount = SystemAPI.GetSingleton<MinProjectileCountComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float spreadHalfMultiplier = SystemAPI.GetSingleton<SpreadHalfMultiplierComponent>().Value;
            float spreadZero = SystemAPI.GetSingleton<SpreadZeroComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            systemState.Dependency = new ShootJob { BulletRotationOffset = bulletRotationOffset , ECBParallelWriter = ecbParallelWriter , MinProjectileCount = minProjectileCount , NoAction = noAction , SpreadHalfMultiplier = spreadHalfMultiplier , SpreadZero = spreadZero , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    public partial struct ShootJob : IJobEntity
    {
        public float BulletRotationOffset;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int MinProjectileCount;
        public int NoAction;
        public float SpreadHalfMultiplier;
        public float SpreadZero;
        public float TimerExpired;

        private void Execute(in BulletEntityComponent bulletEntityComponent , RefRW<CooldownComponent> cooldownComponent , in DamageComponent damageComponent , [EntityIndexInQuery] int entityIndexInQuery , in AttackRateComponent attackRateComponent , in LocalToWorld localToWorld , in ProjectileCountComponent projectileCountComponent , in SpreadComponent spreadComponent , in TargetPositionComponent targetPositionComponent)
        {
            // Check Condition
            bool isReady = cooldownComponent.ValueRO.Value <= TimerExpired;

            // Reset Entity 
            // If isReady is true, set to Entity. Otherwise, keep current negative value.
            // This IMMEDIATE write prevents the "x3 Bug" in the next physics sub-step.
            cooldownComponent.ValueRW.Value = math.select(cooldownComponent.ValueRO.Value , attackRateComponent.Value , isReady);

            // Calculate Loop Value
            // If not ready, count is 0. Loop will not run.
            int spawnCount = math.select(NoAction , projectileCountComponent.Value , isReady);

            for(var i = 0 ; i < spawnCount ; i++)
            {
                Entity newBullet = ECBParallelWriter.Instantiate(entityIndexInQuery , bulletEntityComponent.Entity);

                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , new DamageComponent { Value = damageComponent.Value });

                float spreadAngle = math.radians(spreadComponent.Value);
                float angleStep = math.select(SpreadZero , spreadAngle / math.max(MinProjectileCount , projectileCountComponent.Value - MinProjectileCount) , projectileCountComponent.Value > MinProjectileCount);
                float baseAngle = math.atan2(targetPositionComponent.Value.y - localToWorld.Position.y , targetPositionComponent.Value.x - localToWorld.Position.x);
                float startOffset = spreadAngle * SpreadHalfMultiplier;

                float finalAngle = baseAngle - startOffset + angleStep * i - BulletRotationOffset;

                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , LocalTransform.FromPositionRotation(localToWorld.Position , quaternion.RotateZ(finalAngle)));

                float velocityAngle = baseAngle - startOffset + angleStep * i;
                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , new VelocityComponent { Value = new float2(math.cos(velocityAngle) , math.sin(velocityAngle)) });
            }
        }
    }
}