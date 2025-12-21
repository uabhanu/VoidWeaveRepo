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

        private void Execute(Entity entity , [EntityIndexInQuery] int sortKey , in LocalToWorld localToWorld , in TargetPositionComponent targetPositionComponent , in TurretDamageComponent turretDamageComponent , in TurretFireRateComponent turretFireRateComponent , in TurretProjectileCountComponent turretProjectileCountComponent , in TurretSpreadComponent turretSpreadComponent , in BulletEntityComponent bulletEntityComponent)
        {
            for(int i = 0 ; i < turretProjectileCountComponent.Count ; i++)
            {
                Entity newBullet = ECB.Instantiate(sortKey , bulletEntityComponent.Entity);

                ECB.SetComponent(sortKey , newBullet , LocalTransform.FromPositionRotation(localToWorld.Position , quaternion.RotateZ((math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - (math.radians(turretSpreadComponent.Degrees) * 0.5f) + (math.select(0 , math.radians(turretSpreadComponent.Degrees) / math.max(1 , turretProjectileCountComponent.Count - 1) , turretProjectileCountComponent.Count > 1) * i)) - math.PI / 2f)));
                ECB.SetComponent(sortKey , newBullet , new MovementInputComponent { Input = new float2(math.cos(math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - math.radians(turretSpreadComponent.Degrees) * 0.5f + math.select(0 , math.radians(turretSpreadComponent.Degrees) / math.max(1 , turretProjectileCountComponent.Count - 1) , turretProjectileCountComponent.Count > 1) * i) , math.sin(math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - math.radians(turretSpreadComponent.Degrees) * 0.5f + math.select(0 , math.radians(turretSpreadComponent.Degrees) / math.max(1 , turretProjectileCountComponent.Count - 1) , turretProjectileCountComponent.Count > 1) * i)) });
                ECB.SetComponent(sortKey , newBullet , new ProjectileDamageComponent { Damage = turretDamageComponent.Damage });
            }

            ECB.AddComponent<ProjectileSpawnedTag>(sortKey , entity);
        }
    }
}