namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial struct RotationSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<FloatToleranceComponent>();
        }
        
        public void OnUpdate(ref SystemState systemState)
        {
            systemState.Dependency = new CombatRotationJob { DeltaTime = SystemAPI.Time.DeltaTime , ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter() , FloatTolerence = SystemAPI.GetSingleton<FloatToleranceComponent>().Value }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new MovementRotationJob { DeltaTime = SystemAPI.Time.DeltaTime , FloatTolerence = SystemAPI.GetSingleton<FloatToleranceComponent>().Value }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(HasTargetTag))]
    public partial struct CombatRotationJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        public float FloatTolerence;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref LocalTransform localTransform , in MinRotationRequiredComponent minRotationRequiredComponent , in RotationOffsetComponent rotationOffsetComponent , in RotationSpeedComponent rotationSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            float3 direction = math.normalizesafe(targetPositionComponent.Value - localTransform.Position);
            float targetAngle = math.atan2(direction.y , direction.x) - math.radians(rotationOffsetComponent.Value);
            quaternion targetRotation = quaternion.RotateZ(targetAngle);

            float angleDifference = math.angle(localTransform.Rotation , targetRotation);
            float step = rotationSpeedComponent.Value * DeltaTime;
            float t = math.select(math.saturate(step / angleDifference) , 1 , angleDifference < FloatTolerence);

            localTransform.Rotation = math.slerp(localTransform.Rotation , targetRotation , t);

            bool isAligned = angleDifference <= math.radians(minRotationRequiredComponent.Value);
            ECB.SetComponentEnabled<RotationCompleteTag>(entityIndexInQuery , entity , isAligned);
        }
    }

    [BurstCompile]
    [WithNone(typeof(HasTargetTag))]
    public partial struct MovementRotationJob : IJobEntity
    {
        public float DeltaTime;
        public float FloatTolerence;

        private void Execute(ref LocalTransform localTransform , in MoveDirectionComponent moveDirectionComponent , in RotationOffsetComponent rotationOffsetComponent , in RotationSpeedComponent rotationSpeedComponent)
        {
            float3 moveDir = moveDirectionComponent.Value;
            bool isMoving = math.lengthsq(moveDir) > FloatTolerence;
            
            float targetAngle = math.atan2(moveDir.y , moveDir.x) - math.radians(rotationOffsetComponent.Value);
            quaternion targetRotation = quaternion.RotateZ(targetAngle);
            float4 selectedRotation = math.select(localTransform.Rotation.value , targetRotation.value , isMoving);
            quaternion finalTarget = new quaternion(selectedRotation);

            float angleDifference = math.angle(localTransform.Rotation , finalTarget);
            float step = rotationSpeedComponent.Value * DeltaTime;

            float t = math.select(math.saturate(step / angleDifference) , 1 , angleDifference < FloatTolerence);

            localTransform.Rotation = math.slerp(localTransform.Rotation , finalTarget , t);
        }
    }
}