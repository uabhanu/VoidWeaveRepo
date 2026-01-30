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
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<DashKeyComponent>();
            systemState.RequireForUpdate<DeployKeyComponent>();
            systemState.RequireForUpdate<DownKeyComponent>();
            systemState.RequireForUpdate<InputDashComponent>();
            systemState.RequireForUpdate<InputDeployComponent>();
            systemState.RequireForUpdate<InputDownComponent>();
            systemState.RequireForUpdate<InputLeftComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<InputRightComponent>();
            systemState.RequireForUpdate<InputTurret1Component>();
            systemState.RequireForUpdate<InputTurret2Component>();
            systemState.RequireForUpdate<InputTurret3Component>();
            systemState.RequireForUpdate<InputUpComponent>();
            systemState.RequireForUpdate<LeftKeyComponent>();
            systemState.RequireForUpdate<RightKeyComponent>();
            systemState.RequireForUpdate<Turret1KeyComponent>();
            systemState.RequireForUpdate<Turret2KeyComponent>();
            systemState.RequireForUpdate<Turret3KeyComponent>();
            systemState.RequireForUpdate<UpKeyComponent>();

            systemState.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            var keyboard = Keyboard.current;
            
            Key dashKey = SystemAPI.GetSingleton<DashKeyComponent>().DashKey;
            uint dashValue = SystemAPI.GetSingleton<InputDashComponent>().InputDash;
            
            Key deployKey = SystemAPI.GetSingleton<DeployKeyComponent>().DeployKey;
            uint deployValue = SystemAPI.GetSingleton<InputDeployComponent>().InputDeployValue;
            
            Key downKey = SystemAPI.GetSingleton<DownKeyComponent>().DownKey;
            uint downValue = SystemAPI.GetSingleton<InputDownComponent>().InputDownValue;
            
            Key leftKey = SystemAPI.GetSingleton<LeftKeyComponent>().LeftKey;
            uint leftValue = SystemAPI.GetSingleton<InputLeftComponent>().InputLeftValue;
            
            uint noneValue = SystemAPI.GetSingleton<InputNoneComponent>().InputNoneValue;
            
            Key rightKey = SystemAPI.GetSingleton<RightKeyComponent>().RightKey;
            uint rightValue = SystemAPI.GetSingleton<InputRightComponent>().InputRightValue;
            
            Key turret1Key = SystemAPI.GetSingleton<Turret1KeyComponent>().Turret1Key;
            uint turret1Value = SystemAPI.GetSingleton<InputTurret1Component>().InputTurret1Value;
            
            Key turret2Key = SystemAPI.GetSingleton<Turret2KeyComponent>().Turret2Key;
            uint turret2Value = SystemAPI.GetSingleton<InputTurret2Component>().InputTurret2Value;
            
            Key turret3Key = SystemAPI.GetSingleton<Turret3KeyComponent>().Turret3Key;
            uint turret3Value = SystemAPI.GetSingleton<InputTurret3Component>().InputTurret3Value;
            
            Key upKey = SystemAPI.GetSingleton<UpKeyComponent>().UpKey;
            uint upValue = SystemAPI.GetSingleton<InputUpComponent>().InputUpValue;
            
            uint selectedInput = noneValue;
            
            selectedInput |= math.select(0 , upValue , keyboard[upKey].isPressed);
            selectedInput |= math.select(0 , downValue , keyboard[downKey].isPressed);
            selectedInput |= math.select(0 , leftValue , keyboard[leftKey].isPressed);
            selectedInput |= math.select(0 , rightValue , keyboard[rightKey].isPressed);
            selectedInput |= math.select(0 , dashValue , keyboard[dashKey].wasPressedThisFrame);
            selectedInput |= math.select(0 , deployValue , keyboard[deployKey].wasPressedThisFrame);
            selectedInput |= math.select(0 , turret1Value , keyboard[turret1Key].wasPressedThisFrame);
            selectedInput |= math.select(0 , turret2Value , keyboard[turret2Key].wasPressedThisFrame);
            selectedInput |= math.select(0 , turret3Value , keyboard[turret3Key].wasPressedThisFrame);
            
            foreach(var playerInputComponent in SystemAPI.Query<RefRW<PlayerInputComponent>>().WithAll<PlayerTag>()) { playerInputComponent.ValueRW.PlayerInput = selectedInput; }
        }
    }
}