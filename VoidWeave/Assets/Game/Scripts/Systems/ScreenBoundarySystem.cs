namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct ScreenBoundarySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BoundaryOffsetComponent>();
            systemState.RequireForUpdate<CameraOrthographicSizeComponent>();
            systemState.RequireForUpdate<OneScaleComponent>();
            systemState.RequireForUpdate<ScreenBoundaryXComponent>();
            systemState.RequireForUpdate<ScreenBoundaryYComponent>();
            systemState.RequireForUpdate<ZeroScaleComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            float boundaryOffset = SystemAPI.GetSingleton<BoundaryOffsetComponent>().Value;
            float cameraSize = SystemAPI.GetSingleton<CameraOrthographicSizeComponent>().Value;

            float oneScale = SystemAPI.GetSingleton<OneScaleComponent>().Value;
            float zeroScale = SystemAPI.GetSingleton<ZeroScaleComponent>().Value;

            float aspectRatio = math.select(Screen.width / math.max(oneScale , Screen.height) , oneScale , Screen.height <= zeroScale);

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