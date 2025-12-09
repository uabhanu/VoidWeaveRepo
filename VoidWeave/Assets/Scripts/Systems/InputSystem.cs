// System to read input and apply it to the Player's MovementInputComponent.
// We must use SystemBase here to access UnityEngine.InputSystem (managed code).

namespace Systems
{
    using Gameplay;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private InputSystem_Actions _inputSystemActions;
        private EntityQuery _playerQuery;

        protected override void OnCreate()
        {
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Player.Enable();

            _playerQuery = GetEntityQuery(ComponentType.ReadWrite<MovementInputComponent>() , ComponentType.ReadOnly<PlayerTag>() , ComponentType.ReadWrite<TurretDeploymentInputComponent>());

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
            float2 moveInput = _inputSystemActions.Player.Move.ReadValue<Vector2>();

            float2 normalizedMoveInput = math.normalizesafe(moveInput);

            foreach((RefRW<DashInputComponent> dashInputComponent , RefRW<MovementInputComponent> movementInputComponent , RefRW<TurretDeploymentInputComponent> turretDeploymentInputComponent) in SystemAPI.Query<RefRW<DashInputComponent> , RefRW<MovementInputComponent> , RefRW<TurretDeploymentInputComponent>>().WithAll<PlayerTag>())
            {
                movementInputComponent.ValueRW.MoveInput = normalizedMoveInput;
                dashInputComponent.ValueRW.IsPressed = dashInput;
                turretDeploymentInputComponent.ValueRW.IsPressed = deployInput;
            }
        }
    }
}