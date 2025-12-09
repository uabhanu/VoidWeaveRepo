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

            new MovementJob { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MovementInputComponent movementInputComponent , in MoveSpeedComponent moveSpeedComponent)
        {
            float2 inputVector = movementInputComponent.MoveInput;
            float speed = moveSpeedComponent.MoveSpeed;

            // Calculate the movement vector (Vector * Speed * Time)
            float2 movementStep = inputVector * speed * DeltaTime;

            // Update the LocalTransform component's position
            localTransform.Position.xy += movementStep;
        }
    }
}