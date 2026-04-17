namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial struct RotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            float deltaTime = SystemAPI.Time.DeltaTime;
            int doAction = (int)SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = (int)SystemAPI.GetSingleton<NoActionComponent>().Value;

            systemState.Dependency = new CombatRotationJob { DeltaTime = deltaTime , DoAction = doAction , ECBParallelWriter = ecb , NoAction = noAction }.ScheduleParallel(systemState.Dependency);
            systemState.Dependency = new MovementRotationJob { DeltaTime = deltaTime , DoAction = doAction }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(HasTargetTag))]
    public partial struct CombatRotationJob : IJobEntity
    {
        public float DeltaTime;
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref LocalTransform localTransform , in MinRotationRequiredComponent minRotationRequiredComponent , in RotationOffsetComponent rotationOffsetComponent , in RotationSpeedComponent rotationSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            float3 direction = math.normalizesafe(targetPositionComponent.Value - localTransform.Position);
            float targetAngle = math.atan2(direction.y , direction.x) - math.radians(rotationOffsetComponent.Value);
            quaternion targetRotation = quaternion.RotateZ(targetAngle);

            float angleDifference = math.angle(localTransform.Rotation , targetRotation);
            float step = rotationSpeedComponent.Value * DeltaTime;
            float t = math.select(math.saturate(step / angleDifference) , (float)DoAction , angleDifference < 0.001f);

            localTransform.Rotation = math.slerp(localTransform.Rotation , targetRotation , t);

            bool isAligned = angleDifference <= math.radians(minRotationRequiredComponent.Value);
            for(var i = 0 ; i < math.select(NoAction , DoAction , isAligned) ; i++) ECBParallelWriter.AddComponent<RotationCompleteTag>(entityIndexInQuery , entity);
            for(var i = 0 ; i < math.select(NoAction , DoAction , !isAligned) ; i++) ECBParallelWriter.RemoveComponent<RotationCompleteTag>(entityIndexInQuery , entity);
        }
    }

    [BurstCompile]
    [WithNone(typeof(HasTargetTag))]
    public partial struct MovementRotationJob : IJobEntity
    {
        public float DeltaTime;
        public int DoAction;

        private void Execute(ref LocalTransform localTransform , in MoveDirectionComponent moveDirectionComponent , in RotationOffsetComponent rotationOffsetComponent , in RotationSpeedComponent rotationSpeedComponent)
        {
            float3 moveDir = moveDirectionComponent.Value;
            bool isMoving = math.lengthsq(moveDir) > 0.001f;
            
            float targetAngle = math.atan2(moveDir.y , moveDir.x) - math.radians(rotationOffsetComponent.Value);
            quaternion targetRotation = quaternion.RotateZ(targetAngle);
            float4 selectedRotation = math.select(localTransform.Rotation.value , targetRotation.value , isMoving);
            quaternion finalTarget = new quaternion(selectedRotation);

            float angleDifference = math.angle(localTransform.Rotation , finalTarget);
            float step = rotationSpeedComponent.Value * DeltaTime;

            float t = math.select(math.saturate(step / angleDifference) , DoAction , angleDifference < 0.001f);

            localTransform.Rotation = math.slerp(localTransform.Rotation , finalTarget , t);
        }
    }
}