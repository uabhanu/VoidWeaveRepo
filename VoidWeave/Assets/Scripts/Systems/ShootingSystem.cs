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

            state.Dependency = new TurretShootJob { DeltaTime = SystemAPI.Time.DeltaTime , EntityCommandBuffer = ecb }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct TurretShootJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        private void Execute(in BulletEntityComponent bulletEntityComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalToWorld localToWorld , in TargetPositionComponent targetPositionComponent , ref TurretCooldownComponent turretCooldownComponent , in TurretDamageComponent turretDamageComponent , in TurretFireRateComponent turretFireRateComponent , in TurretProjectileCountComponent turretProjectileCountComponent , in TurretRangeComponent turretRangeComponent , in TurretSpreadComponent turretSpreadComponent)
        {
            turretCooldownComponent.Timer -= DeltaTime;
            
            for(int i = 0 ; i < math.select(0 , turretProjectileCountComponent.Count , (turretCooldownComponent.Timer <= 0f) && (math.lengthsq(targetPositionComponent.Position) > 0.001f) && (math.distancesq(localToWorld.Position.xy , targetPositionComponent.Position.xy) <= turretRangeComponent.Range * turretRangeComponent.Range)) ; i++)
            {
                Entity newBullet = EntityCommandBuffer.Instantiate(entityInQueryIndex , bulletEntityComponent.Entity);

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , LocalTransform.FromPositionRotation(localToWorld.Position , quaternion.RotateZ((math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - (math.radians(turretSpreadComponent.Degrees) * 0.5f) + ((math.radians(turretSpreadComponent.Degrees) / math.max(1 , turretProjectileCountComponent.Count - 1)) * i)) - math.PI / 2f)));
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , new MovementInputComponent { Input = new float2(math.cos((math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - (math.radians(turretSpreadComponent.Degrees) * 0.5f) + ((math.radians(turretSpreadComponent.Degrees) / math.max(1 , turretProjectileCountComponent.Count - 1)) * i))) , math.sin((math.atan2(targetPositionComponent.Position.y - localToWorld.Position.y , targetPositionComponent.Position.x - localToWorld.Position.x) - (math.radians(turretSpreadComponent.Degrees) * 0.5f) + ((math.radians(turretSpreadComponent.Degrees) / math.max(1 , turretProjectileCountComponent.Count - 1)) * i)))) });
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newBullet , new ProjectileDamageComponent { Damage = turretDamageComponent.Damage });
            }
            
            turretCooldownComponent.Timer = math.select(math.max(turretCooldownComponent.Timer , -0.01f) , turretFireRateComponent.Rate , (turretCooldownComponent.Timer <= 0f) && (math.lengthsq(targetPositionComponent.Position) > 0.001f) && (math.distancesq(localToWorld.Position.xy , targetPositionComponent.Position.xy) <= turretRangeComponent.Range * turretRangeComponent.Range));
        }
    }
}