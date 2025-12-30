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
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new ShootJob { ECB = ecb }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(CanShootTag))]
    public partial struct ShootJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(in BulletEntityComponent bulletEntityComponent , in DamageComponent damageComponent , Entity entity , [EntityIndexInQuery] int sortKey , in FireRateComponent fireRateComponent , in LocalToWorld localToWorld , in ProjectileCountComponent projectileCountComponent , in TargetPositionComponent targetPositionComponent , in SpreadComponent spreadComponent)
        {
            for(int i = 0 ; i < projectileCountComponent.Count ; i++)
            {
                Entity newBullet = ECB.Instantiate(sortKey , bulletEntityComponent.Entity);
                
                // math.atan2(...) -> Gets the direct angle to the target.
                // - (math.radians(spreadComponent.Degrees) * 0.5f) -> RETRACTS the angle by half the spread.
                // (Reason: This * 0.5f is mandatory to CENTER the fan. Without it, bullets start at the target and fan only to one side.)
                // (... * i) -> Steps forward through the arc for each bullet.

                ECB.SetComponent(sortKey , newBullet , new DamageComponent { Damage = damageComponent.Damage });
                
                // Rotation (Includes -PI/2 for Sprite Alignment)
                ECB.SetComponent(sortKey , newBullet , LocalTransform.FromPositionRotation(localToWorld.Position , quaternion.RotateZ((math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - (math.radians(spreadComponent.Degrees) * 0.5f) + (math.select(0 , math.radians(spreadComponent.Degrees) / math.max(1 , projectileCountComponent.Count - 1) , projectileCountComponent.Count > 1) * i)) - math.PI / 2f)));
                
                ECB.SetComponent(sortKey , newBullet , new VelocityComponent { Velocity = new float2(math.cos(math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - math.radians(spreadComponent.Degrees) * 0.5f + math.select(0 , math.radians(spreadComponent.Degrees) / math.max(1 , projectileCountComponent.Count - 1) , projectileCountComponent.Count > 1) * i) , math.sin(math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - math.radians(spreadComponent.Degrees) * 0.5f + math.select(0 , math.radians(spreadComponent.Degrees) / math.max(1 , projectileCountComponent.Count - 1) , projectileCountComponent.Count > 1) * i)) });
            }

            ECB.AddComponent<ProjectileSpawnedEventTag>(sortKey , entity);
        }
    }
}