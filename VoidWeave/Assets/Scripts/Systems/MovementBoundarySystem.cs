namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct MovementBoundarySystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            new MovementBoundaryJob { BoundaryX = (5.0f * ((float)UnityEngine.Screen.width / UnityEngine.Screen.height)) - 0.9f , BoundaryY = 5.0f - 0.9f }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MovementBoundaryJob : IJobEntity
    {
        public float BoundaryX;
        public float BoundaryY;
        
        private void Execute(in LocalTransform localTransform , ref MovementInputComponent movementInputComponent , in PlayerTag playerTag)
        {
            movementInputComponent.Input.x = (math.max(0 , movementInputComponent.Input.x) * math.step(localTransform.Position.x , BoundaryX)) + (math.min(0 , movementInputComponent.Input.x) * math.step(-BoundaryX , localTransform.Position.x));
            movementInputComponent.Input.y = (math.max(0 , movementInputComponent.Input.y) * math.step(localTransform.Position.y , BoundaryY)) + (math.min(0 , movementInputComponent.Input.y) * math.step(-BoundaryY , localTransform.Position.y));
        }
    }
}