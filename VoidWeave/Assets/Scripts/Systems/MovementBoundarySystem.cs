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
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BoundaryOffsetComponent>();
            systemState.RequireForUpdate<CameraOrthographicSizeComponent>();
            systemState.RequireForUpdate<InputDownComponent>();
            systemState.RequireForUpdate<InputLeftComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<InputRightComponent>();
            systemState.RequireForUpdate<InputUpComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            float boundaryOffset = SystemAPI.GetSingleton<BoundaryOffsetComponent>().Offset;
            float cameraSize = SystemAPI.GetSingleton<CameraOrthographicSizeComponent>().Size;

            // Input Masks
            uint inputDown = SystemAPI.GetSingleton<InputDownComponent>().InputDown;
            uint inputLeft = SystemAPI.GetSingleton<InputLeftComponent>().InputLeft;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().InputNone;
            uint inputRight = SystemAPI.GetSingleton<InputRightComponent>().InputRight;
            uint inputUp = SystemAPI.GetSingleton<InputUpComponent>().InputUp;

            // Screen Calculation
            float aspect = (float)UnityEngine.Screen.width / UnityEngine.Screen.height;
            float boundaryX = cameraSize * aspect - boundaryOffset;
            float boundaryY = cameraSize - boundaryOffset;

            new MovementBoundaryJob { BoundaryX = boundaryX , BoundaryY = boundaryY , InputDown = inputDown , InputLeft = inputLeft , InputNone = inputNone , InputRight = inputRight , InputUp = inputUp }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MovementBoundaryJob : IJobEntity
    {
        public float BoundaryX;
        public float BoundaryY;
        public uint InputDown;
        public uint InputLeft;
        public uint InputNone;
        public uint InputRight;
        public uint InputUp;

        private void Execute(in LocalTransform localTransform , ref PlayerInputComponent playerInputComponent , in PlayerTag playerTag)
        {
            // Mapping: Up=1, Down=2, Left=4, Right=8

            // If Position.x >= BoundaryX, remove Right bit (~8u). Otherwise keep all (~0u).
            playerInputComponent.PlayerInput &= math.select(~InputNone , ~InputRight , localTransform.Position.x >= BoundaryX);

            // If Position.x <= -BoundaryX, remove Left bit (~4u).
            playerInputComponent.PlayerInput &= math.select(~InputNone , ~InputLeft , localTransform.Position.x <= -BoundaryX);

            // If Position.y >= BoundaryY, remove Up bit (~1u).
            playerInputComponent.PlayerInput &= math.select(~InputNone , ~InputUp , localTransform.Position.y >= BoundaryY);

            // If Position.y <= -BoundaryY, remove Down bit (~2u).
            playerInputComponent.PlayerInput &= math.select(~InputNone , ~InputDown , localTransform.Position.y <= -BoundaryY);
        }
    }
}