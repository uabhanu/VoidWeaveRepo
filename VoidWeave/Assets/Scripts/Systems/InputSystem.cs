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

        protected override void OnDestroy() { _inputSystemActions.Player.Disable(); }

        protected override void OnUpdate()
        {
            foreach((RefRW<DashInputComponent> dashInputComponent , RefRW<MovementInputComponent> movementInputComponent , RefRW<ScatterTurretInputComponent> scatterTurretInputComponent , RefRW<StrikerTurretInputComponent> strikerTurretInputComponent , RefRW<TurretDeploymentInputComponent> turretDeploymentInputComponent) in SystemAPI.Query<RefRW<DashInputComponent> , RefRW<MovementInputComponent> , RefRW<ScatterTurretInputComponent> , RefRW<StrikerTurretInputComponent> , RefRW<TurretDeploymentInputComponent>>().WithAll<PlayerTag>())
            {
                dashInputComponent.ValueRW.IsPressed = _inputSystemActions.Player.Dash.ReadValue<float>();
                movementInputComponent.ValueRW.Input = math.normalizesafe(_inputSystemActions.Player.Move.ReadValue<Vector2>());
                scatterTurretInputComponent.ValueRW.Input = _inputSystemActions.Player.ScatterTurret.ReadValue<float>();
                strikerTurretInputComponent.ValueRW.Input = _inputSystemActions.Player.StrikerTurret.ReadValue<float>();
                turretDeploymentInputComponent.ValueRW.IsPressed = math.select(0f , 1f , _inputSystemActions.Player.Deploy.WasPressedThisFrame());
            }
        }
    }
}