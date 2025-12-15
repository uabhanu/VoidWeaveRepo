// System to read input and apply it to the Player's MovementInputComponent.
// We must use SystemBase here to access UnityEngine.InputSystem (managed code).

namespace Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private InputSystem_Actions _inputSystemActions;

        protected override void OnCreate()
        {
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Player.Enable();
            
            RequireForUpdate<PlayerTag>();
            RequireForUpdate<TurretDeploymentInputComponent>();
        }

        protected override void OnDestroy()
        {
            _inputSystemActions.Player.Disable();
        }

        protected override void OnUpdate()
        {
            float dashInput = _inputSystemActions.Player.Dash.ReadValue<float>();
            float deployInput = math.select(0f , 1f , _inputSystemActions.Player.Deploy.WasPressedThisFrame());
            float scatterTurretInput = _inputSystemActions.Player.ScatterTurret.ReadValue<float>();
            float strikerTurretInput = _inputSystemActions.Player.StrikerTurret.ReadValue<float>();
            
            float2 moveInput = _inputSystemActions.Player.Move.ReadValue<Vector2>();
            float2 normalizedMoveInput = math.normalizesafe(moveInput);

            foreach((RefRW<DashInputComponent> dashInputComponent , RefRW<MovementInputComponent> movementInputComponent , RefRW<ScatterTurretInputComponent> scatterTurretInputComponent , RefRW<StrikerTurretInputComponent> strikerTurretInputComponent , RefRW<TurretDeploymentInputComponent> turretDeploymentInputComponent) in SystemAPI.Query<RefRW<DashInputComponent> , RefRW<MovementInputComponent> , RefRW<ScatterTurretInputComponent> , RefRW<StrikerTurretInputComponent> , RefRW<TurretDeploymentInputComponent>>().WithAll<PlayerTag>())
            {
                movementInputComponent.ValueRW.Input = normalizedMoveInput;
                dashInputComponent.ValueRW.IsPressed = dashInput;
                scatterTurretInputComponent.ValueRW.Input = scatterTurretInput;
                strikerTurretInputComponent.ValueRW.Input = strikerTurretInput;
                turretDeploymentInputComponent.ValueRW.IsPressed = deployInput;
            }
        }
    }
}