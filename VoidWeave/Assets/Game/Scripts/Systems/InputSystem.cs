namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct InputSystem : ISystem
    {
        private EntityQuery _tutorialActiveQuery;
        
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
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<RightKeyComponent>();
            systemState.RequireForUpdate<Turret1KeyComponent>();
            systemState.RequireForUpdate<Turret2KeyComponent>();
            systemState.RequireForUpdate<Turret3KeyComponent>();
            systemState.RequireForUpdate<UpKeyComponent>();

            systemState.RequireForUpdate<PlayerTag>();
            
            _tutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag , TurretsTutorialActiveTag>().Build();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            int currentLevel = SystemAPI.GetSingleton<LevelComponent>().Value;
            bool isTutorialActive = !_tutorialActiveQuery.IsEmpty;
            
            bool canPress1 = !isTutorialActive || currentLevel == 1 || currentLevel >= 4;
            bool canPress2 = !isTutorialActive || currentLevel == 2 || currentLevel >= 4;
            bool canPress3 = !isTutorialActive || currentLevel == 3 || currentLevel >= 4;
            
            Keyboard keyboard = Keyboard.current;

            Key dashKey = SystemAPI.GetSingleton<DashKeyComponent>().Value;
            uint dashValue = SystemAPI.GetSingleton<InputDashComponent>().Value;

            Key deployKey = SystemAPI.GetSingleton<DeployKeyComponent>().Value;
            uint deployValue = SystemAPI.GetSingleton<InputDeployComponent>().Value;

            Key downKey = SystemAPI.GetSingleton<DownKeyComponent>().Value;
            uint downValue = SystemAPI.GetSingleton<InputDownComponent>().Value;

            Key leftKey = SystemAPI.GetSingleton<LeftKeyComponent>().Value;
            uint leftValue = SystemAPI.GetSingleton<InputLeftComponent>().Value;

            uint noneValue = SystemAPI.GetSingleton<InputNoneComponent>().Value;

            Key rightKey = SystemAPI.GetSingleton<RightKeyComponent>().Value;
            uint rightValue = SystemAPI.GetSingleton<InputRightComponent>().Value;

            Key turret1Key = SystemAPI.GetSingleton<Turret1KeyComponent>().Value;
            uint turret1Value = SystemAPI.GetSingleton<InputTurret1Component>().Value;

            Key turret2Key = SystemAPI.GetSingleton<Turret2KeyComponent>().Value;
            uint turret2Value = SystemAPI.GetSingleton<InputTurret2Component>().Value;

            Key turret3Key = SystemAPI.GetSingleton<Turret3KeyComponent>().Value;
            uint turret3Value = SystemAPI.GetSingleton<InputTurret3Component>().Value;

            Key upKey = SystemAPI.GetSingleton<UpKeyComponent>().Value;
            uint upValue = SystemAPI.GetSingleton<InputUpComponent>().Value;

            uint selectedInput = noneValue;

            selectedInput |= math.select(noneValue , upValue , keyboard[upKey].isPressed);
            selectedInput |= math.select(noneValue , downValue , keyboard[downKey].isPressed);
            selectedInput |= math.select(noneValue , leftValue , keyboard[leftKey].isPressed);
            selectedInput |= math.select(noneValue , rightValue , keyboard[rightKey].isPressed);
            selectedInput |= math.select(noneValue , dashValue , keyboard[dashKey].wasPressedThisFrame);
            selectedInput |= math.select(noneValue , deployValue , keyboard[deployKey].wasPressedThisFrame);
            selectedInput |= math.select(noneValue , turret1Value , keyboard[turret1Key].wasPressedThisFrame && canPress1);
            selectedInput |= math.select(noneValue , turret2Value , keyboard[turret2Key].wasPressedThisFrame && canPress2);
            selectedInput |= math.select(noneValue , turret3Value , keyboard[turret3Key].wasPressedThisFrame && canPress3);

            foreach(RefRW<PlayerInputComponent> playerInputComponent in SystemAPI.Query<RefRW<PlayerInputComponent>>().WithAll<PlayerTag>()) playerInputComponent.ValueRW.Value = selectedInput;
        }
    }
}