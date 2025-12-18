namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AIMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
            new InputMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(SeekerTag))]
    public partial struct AIMovementJob : IJobEntity
    {
        public float DeltaTime;
        
        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            localTransform.Position.xy += math.normalizesafe(targetPositionComponent.Position - localTransform.Position).xy * moveSpeedComponent.Speed * DeltaTime;
        }
    }
    
    [BurstCompile]
    [WithNone(typeof(SeekerTag))]
    public partial struct InputMovementJob : IJobEntity
    {
        public float DeltaTime;
        
        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in MovementInputComponent movementInputComponent) { localTransform.Position.xy += movementInputComponent.Input * moveSpeedComponent.Speed * DeltaTime; }
    }
}