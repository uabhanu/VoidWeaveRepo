namespace Systems
{
    using Gameplay;
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
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            new GuidedMovementJob { DeltaTime = deltaTime }.ScheduleParallel();
            new InputMovementJob { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    // Runs on Player (No SeekerTag)
    [BurstCompile]
    [WithNone(typeof(SeekerTag))]
    public partial struct InputMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MovementInputComponent movementInputComponent , in MoveSpeedComponent moveSpeedComponent)
        {
            float2 inputVector = movementInputComponent.MoveInput;
            float speed = moveSpeedComponent.MoveSpeed;

            localTransform.Position.xy += inputVector * speed * DeltaTime;
        }
    }

    // Runs on AI (SeekerTag)
    // Directly calculates direction from TargetPosition, replacing GuidanceSystem
    [BurstCompile]
    [WithAll(typeof(SeekerTag))]
    public partial struct GuidedMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            float3 currentPos = localTransform.Position;
            float3 targetPos = targetPositionComponent.TargetPosition;
            float speed = moveSpeedComponent.MoveSpeed;

            // 1. Calculate Direction
            float3 direction = targetPos - currentPos;
            float2 normalizedDir = math.normalizesafe(direction).xy;

            // 2. Move
            localTransform.Position.xy += normalizedDir * speed * DeltaTime;
        }
    }
}