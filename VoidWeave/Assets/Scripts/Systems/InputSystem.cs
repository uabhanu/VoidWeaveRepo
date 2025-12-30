namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct InputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<PlayerTag>(); }

        public void OnUpdate(ref SystemState state)
        {
            var keyboard = Keyboard.current;

            uint selectedInput = 0;

            // Directions
            selectedInput |= (uint)math.select(0 , 1 , keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed); // Up
            selectedInput |= (uint)math.select(0 , 2 , keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed); // Down
            selectedInput |= (uint)math.select(0 , 4 , keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed); // Left
            selectedInput |= (uint)math.select(0 , 8 , keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed); // Right

            // Actions
            selectedInput |= (uint)math.select(0 , 16 , keyboard.leftShiftKey.wasPressedThisFrame); // Dash
            selectedInput |= (uint)math.select(0 , 32 , keyboard.spaceKey.wasPressedThisFrame); // Deploy
            selectedInput |= (uint)math.select(0 , 64 , keyboard.digit1Key.wasPressedThisFrame); // Striker
            selectedInput |= (uint)math.select(0 , 128 , keyboard.digit2Key.wasPressedThisFrame); // Scatter

            foreach(var input in SystemAPI.Query<RefRW<PlayerInputComponent>>().WithAll<PlayerTag>()) { input.ValueRW.SelectedInput = selectedInput; }
        }
    }
}