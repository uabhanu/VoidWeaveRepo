namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct ScreenBoundarySystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BoundaryOffsetComponent>();
            systemState.RequireForUpdate<CameraOrthographicSizeComponent>();
            systemState.RequireForUpdate<ScreenBoundaryXComponent>();
            systemState.RequireForUpdate<ScreenBoundaryYComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            float boundaryOffset = SystemAPI.GetSingleton<BoundaryOffsetComponent>().Value;
            float cameraSize = SystemAPI.GetSingleton<CameraOrthographicSizeComponent>().Value;

            float aspectRatio = math.select(Screen.width / math.max(1f , Screen.height) , 1f , Screen.height <= 0f);

            float boundaryX = cameraSize * aspectRatio - boundaryOffset;
            float boundaryY = cameraSize - boundaryOffset;

            SystemAPI.SetSingleton(new ScreenBoundaryXComponent { Value = boundaryX });
            SystemAPI.SetSingleton(new ScreenBoundaryYComponent { Value = boundaryY });

            new ScreenBoundaryJob { BoundaryX = boundaryX , BoundaryY = boundaryY }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(ProjectileTag))]
    public partial struct ScreenBoundaryJob : IJobEntity
    {
        public float BoundaryX;
        public float BoundaryY;
        
        private void Execute(ref LocalTransform localTransform)
        {
            localTransform.Position.x = math.clamp(localTransform.Position.x , -BoundaryX , BoundaryX);
            localTransform.Position.y = math.clamp(localTransform.Position.y , -BoundaryY , BoundaryY);
        }
    }
}