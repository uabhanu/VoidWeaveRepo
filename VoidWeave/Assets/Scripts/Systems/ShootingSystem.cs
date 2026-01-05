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
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            systemState.Dependency = new ShootJob { ECB = ecb }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    public partial struct ShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        
        private void Execute(in BulletEntityComponent bulletEntityComponent , RefRW<CooldownComponent> cooldownComponent , in DamageComponent damageComponent , [EntityIndexInQuery] int entityIndexInQuery , in AttackRateComponent attackRateComponent , in LocalToWorld localToWorld , in ProjectileCountComponent projectileCountComponent , in SpreadComponent spreadComponent , in TargetPositionComponent targetPositionComponent)
        {
            // Check Condition
            bool isReady = cooldownComponent.ValueRO.Timer <= 0;

            // Reset Timer 
            // If isReady is true, set to AttackRate. Otherwise, keep current negative value.
            // This IMMEDIATE write prevents the "x3 Bug" in the next physics sub-step.
            cooldownComponent.ValueRW.Timer = math.select(cooldownComponent.ValueRO.Timer , attackRateComponent.AttackRate , isReady);

            // Calculate Loop Count
            // If not ready, count is 0. Loop will not run.
            int spawnCount = math.select(0 , projectileCountComponent.Count , isReady);

            for(int i = 0 ; i < spawnCount ; i++)
            {
                Entity newBullet = ECB.Instantiate(entityIndexInQuery , bulletEntityComponent.Entity);

                ECB.SetComponent(entityIndexInQuery , newBullet , new DamageComponent { Damage = damageComponent.Damage });

                // Rotation Logic
                ECB.SetComponent(entityIndexInQuery , newBullet , LocalTransform.FromPositionRotation(localToWorld.Position , quaternion.RotateZ((math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - (math.radians(spreadComponent.Degrees) * 0.5f) + (math.select(0 , math.radians(spreadComponent.Degrees) / math.max(1 , projectileCountComponent.Count - 1) , projectileCountComponent.Count > 1) * i)) - math.PI / 2f)));

                ECB.SetComponent(entityIndexInQuery , newBullet , new VelocityComponent { Velocity = new float2(math.cos(math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - math.radians(spreadComponent.Degrees) * 0.5f + math.select(0 , math.radians(spreadComponent.Degrees) / math.max(1 , projectileCountComponent.Count - 1) , projectileCountComponent.Count > 1) * i) , math.sin(math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - math.radians(spreadComponent.Degrees) * 0.5f + math.select(0 , math.radians(spreadComponent.Degrees) / math.max(1 , projectileCountComponent.Count - 1) , projectileCountComponent.Count > 1) * i)) });
            }
        }
    }
}