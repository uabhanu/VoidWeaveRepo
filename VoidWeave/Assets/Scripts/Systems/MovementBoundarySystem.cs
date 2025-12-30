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
        public void OnUpdate(ref SystemState state) { new MovementBoundaryJob { BoundaryX = (5.0f * ((float)UnityEngine.Screen.width / UnityEngine.Screen.height)) - 0.9f , BoundaryY = 5.0f - 0.9f }.ScheduleParallel(); }
    }

    [BurstCompile]
    public partial struct MovementBoundaryJob : IJobEntity
    {
        public float BoundaryX;
        public float BoundaryY;

        private void Execute(in LocalTransform localTransform , ref PlayerInputComponent playerInputComponent , in PlayerTag playerTag)
        {
            // Mapping: Up=1, Down=2, Left=4, Right=8

            // If Position.x >= BoundaryX, remove Right bit (~8u). Otherwise keep all (~0u).
            playerInputComponent.SelectedInput &= math.select(~0u , ~8u , localTransform.Position.x >= BoundaryX);

            // If Position.x <= -BoundaryX, remove Left bit (~4u).
            playerInputComponent.SelectedInput &= math.select(~0u , ~4u , localTransform.Position.x <= -BoundaryX);

            // If Position.y >= BoundaryY, remove Up bit (~1u).
            playerInputComponent.SelectedInput &= math.select(~0u , ~1u , localTransform.Position.y >= BoundaryY);

            // If Position.y <= -BoundaryY, remove Down bit (~2u).
            playerInputComponent.SelectedInput &= math.select(~0u , ~2u , localTransform.Position.y <= -BoundaryY);
        }
    }
}