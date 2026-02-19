namespace Game.Scripts.Systems
{
    using Game.Scripts.Components;
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
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float spreadHalfMultiplier = SystemAPI.GetSingleton<SpreadHalfMultiplierComponent>().Value;
            float spreadZero = SystemAPI.GetSingleton<SpreadZeroComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;

            systemState.Dependency = new ShootJob { BulletRotationOffset = bulletRotationOffset , ECBParallelWriter = ecbParallelWriter , NoAction = noAction , SpreadHalfMultiplier = spreadHalfMultiplier , SpreadZero = spreadZero , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    public partial struct ShootJob : IJobEntity
    {
        public float BulletRotationOffset;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;
        public float SpreadHalfMultiplier;
        public float SpreadZero;
        public float TimerExpired;

        private void Execute(in AttackRateComponent attackRateComponent , in BulletEntityComponent bulletEntityComponent , RefRW<CooldownComponent> cooldownComponent , in DamageComponent damageComponent , [EntityIndexInQuery] int entityIndexInQuery , in LocalToWorld localToWorld , in NozzleOffsetComponent nozzleOffsetComponent , in ProjectileCountComponent projectileCountComponent , in SpreadComponent spreadComponent , in TargetPositionComponent targetPositionComponent)
        {
            bool isReady = cooldownComponent.ValueRO.Value <= TimerExpired;
            cooldownComponent.ValueRW.Value = math.select(cooldownComponent.ValueRO.Value , attackRateComponent.Value , isReady);

            int spawnCount = math.select(NoAction , projectileCountComponent.Value , isReady);

            // Transform the local nozzle offset into world space based on the turret's current orientation
            float3 spawnWorldPos = math.transform(localToWorld.Value , nozzleOffsetComponent.Value);

            for(var i = 0 ; i < spawnCount ; i++)
            {
                Entity newBullet = ECBParallelWriter.Instantiate(entityIndexInQuery , bulletEntityComponent.Entity);

                float spreadAngle = math.radians(spreadComponent.Value);

                // Calculate step using the existing ProjectileCount to determine fanning
                float angleStep = math.select(SpreadZero , spreadAngle / math.max(1 , projectileCountComponent.Value - 1) , projectileCountComponent.Value > 1);
                float startOffset = spreadAngle * SpreadHalfMultiplier;

                // Combine the turret's world orientation with the sprite's rotation and spread offsets to ensure the bullet points forward
                float currentAngleOffset = BulletRotationOffset - startOffset + (angleStep * i);
                quaternion finalRotation = math.mul(localToWorld.Rotation , quaternion.RotateZ(currentAngleOffset));

                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , new DamageComponent { Value = damageComponent.Value });

                // Spawn the bullet at the dynamically calculated world position
                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , LocalTransform.FromPositionRotation(spawnWorldPos , finalRotation));

                // Set velocity based on the final calculated rotation so it moves where it looks
                float3 direction = math.mul(finalRotation , math.up());
                ECBParallelWriter.SetComponent(entityIndexInQuery , newBullet , new VelocityComponent { Value = direction.xy });
            }
        }
    }
}