namespace Systems
{
    using Gameplay;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct MovementBoundarySystem : ISystem
    {
        // Define the REAL Camera Size (usually 5 in Unity 2D)
        private const float CAMERA_ORTHO_SIZE = 5.0f;

        // Define how much space the player needs (Radius)
        private const float PLAYER_PADDING = 0.9f;

        public void OnUpdate(ref SystemState state)
        {
            // Calculate Aspect Ratio
            float aspect = (float)UnityEngine.Screen.width / UnityEngine.Screen.height;

            // --- CALCULATE Y LIMIT ---
            // Camera Height (5) minus Padding (0.5) = 4.5
            float yLimit = CAMERA_ORTHO_SIZE - PLAYER_PADDING;

            // --- CALCULATE X LIMIT ---
            // Get Full Screen Width (e.g., 5 * 1.77 = 8.88)
            float fullScreenWidth = CAMERA_ORTHO_SIZE * aspect;

            // Subtract Padding (8.88 - 0.5 = 8.38)
            // This lets the player go much further, right up to the edge minus their body size.
            float xLimit = fullScreenWidth - PLAYER_PADDING;

            // Pass to Job
            new MovementBoundaryJob { ArenaLimitX = xLimit , ArenaLimitY = yLimit }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MovementBoundaryJob : IJobEntity
    {
        public float ArenaLimitX;
        public float ArenaLimitY;

        private void Execute(ref MovementInputComponent inputComponent , in LocalTransform localTransform)
        {
            float2 input = inputComponent.MoveInput;
            float3 position = localTransform.Position;

            // X AXIS
            float canGoRight = math.step(position.x , ArenaLimitX);
            float canGoLeft = math.step(-ArenaLimitX , position.x);

            float inputRight = math.max(0 , input.x) * canGoRight;
            float inputLeft = math.min(0 , input.x) * canGoLeft;
            input.x = inputRight + inputLeft;

            // Y AXIS
            float canGoUp = math.step(position.y , ArenaLimitY);
            float canGoDown = math.step(-ArenaLimitY , position.y);

            float inputUp = math.max(0 , input.y) * canGoUp;
            float inputDown = math.min(0 , input.y) * canGoDown;
            input.y = inputUp + inputDown;

            inputComponent.MoveInput = input;
        }
    }
}