namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

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
            float boundaryOffset = SystemAPI.GetSingleton<BoundaryOffsetComponent>().Value;
            float cameraSize = SystemAPI.GetSingleton<CameraOrthographicSizeComponent>().Value;

            // Input Masks
            uint inputDown = SystemAPI.GetSingleton<InputDownComponent>().Value;
            uint inputLeft = SystemAPI.GetSingleton<InputLeftComponent>().Value;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().Value;
            uint inputRight = SystemAPI.GetSingleton<InputRightComponent>().Value;
            uint inputUp = SystemAPI.GetSingleton<InputUpComponent>().Value;

            // Screen Calculation
            float aspect = (float)Screen.width / Screen.height;
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

            // If Value.x >= BoundaryX, remove Right bit (~8u). Otherwise keep all (~0u).
            playerInputComponent.Value &= math.select(~InputNone , ~InputRight , localTransform.Position.x >= BoundaryX);

            // If Value.x <= -BoundaryX, remove Left bit (~4u).
            playerInputComponent.Value &= math.select(~InputNone , ~InputLeft , localTransform.Position.x <= -BoundaryX);

            // If Value.y >= BoundaryY, remove Up bit (~1u).
            playerInputComponent.Value &= math.select(~InputNone , ~InputUp , localTransform.Position.y >= BoundaryY);

            // If Value.y <= -BoundaryY, remove Down bit (~2u).
            playerInputComponent.Value &= math.select(~InputNone , ~InputDown , localTransform.Position.y <= -BoundaryY);
        }
    }
}