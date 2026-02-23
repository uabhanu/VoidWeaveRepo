namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial struct TurretRotationSystem : ISystem
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
            EntityCommandBuffer.ParallelWriter ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            systemState.Dependency = new TurretRotationJob { DeltaTime = SystemAPI.Time.DeltaTime , DoAction = SystemAPI.GetSingleton<DoActionComponent>().Value , ECBParallelWriter = ecbParallelWriter , NoAction = SystemAPI.GetSingleton<NoActionComponent>().Value }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(HasTargetTag))]
    public partial struct TurretRotationJob : IJobEntity
    {
        public float DeltaTime;
        public int DoAction;
        public EntityCommandBuffer.ParallelWriter ECBParallelWriter;
        public int NoAction;

        private void Execute(Entity entity , [EntityIndexInQuery] int entityIndexInQuery , ref LocalTransform localTransform , in MinRotationRequiredComponent minRotationRequiredComponent , in RotationOffsetComponent rotationOffsetComponent , in RotationSpeedComponent rotationSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            float3 direction = math.normalize(targetPositionComponent.Value - localTransform.Position);
            float targetAngle = math.atan2(direction.y , direction.x) - math.radians(rotationOffsetComponent.Value);
            quaternion targetRotation = quaternion.RotateZ(targetAngle);

            float angleDifference = math.angle(localTransform.Rotation , targetRotation);
            float step = rotationSpeedComponent.Value * DeltaTime;
            float t = math.select(step / angleDifference , DoAction , angleDifference < step);

            localTransform.Rotation = math.slerp(localTransform.Rotation , targetRotation , t);

            float3 currentDir = math.mul(localTransform.Rotation , math.up());
            float rotationDot = math.dot(currentDir , direction);
            bool isAligned = rotationDot >= minRotationRequiredComponent.Value;

            for(var i = 0 ; i < math.select(NoAction , DoAction , isAligned) ; i++) ECBParallelWriter.AddComponent<RotationCompleteTag>(entityIndexInQuery , entity);
            for(var i = 0 ; i < math.select(NoAction , DoAction , !isAligned) ; i++) ECBParallelWriter.RemoveComponent<RotationCompleteTag>(entityIndexInQuery , entity);
        }
    }
}