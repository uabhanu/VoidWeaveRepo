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
            
            _playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>() , ComponentType.ReadWrite<MovementInputComponent>());
            
            RequireForUpdate<PlayerTag>();
        }

        protected override void OnDestroy()
        {
            // Clean up managed resources
            if(_inputSystemActions != null) { _inputSystemActions.Player.Disable(); }
        }

        protected override void OnUpdate()
        {
            float dashInput = _inputSystemActions.Player.Dash.ReadValue<float>();
            float2 moveInput = _inputSystemActions.Player.Move.ReadValue<Vector2>();
            float2 normalizedMoveInput = math.normalizesafe(moveInput);
            
            Entity playerEntity = _playerQuery.GetSingletonEntity();
            
            EntityManager.SetComponentData(playerEntity , new DashInputComponent { IsPressed = dashInput });
            EntityManager.SetComponentData(playerEntity , new MovementInputComponent { MoveInput = normalizedMoveInput });
        }
    }
}