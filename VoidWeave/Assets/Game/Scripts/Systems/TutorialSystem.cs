namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(LevelProgressionSystem))]
    public partial struct TutorialSystem : ISystem
    {
        private EntityQuery _advanceLevelQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<InputDashComponent>();
            systemState.RequireForUpdate<InputDeployComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<Level1EnergyForTutorialComponent>();
            systemState.RequireForUpdate<Level2EnergyForTutorialComponent>();
            systemState.RequireForUpdate<Level3EnergyForTutorialComponent>();
            systemState.RequireForUpdate<MaxLevelsForTutorialsComponent>();
            systemState.RequireForUpdate<MovementNoneComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<PlayerInputComponent>();
            systemState.RequireForUpdate<SelectedTurretEntityComponent>();

            _advanceLevelQuery = SystemAPI.QueryBuilder().WithAll<AdvanceLevelEventTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int currentLevel = SystemAPI.GetSingleton<LevelComponent>().Value;
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            uint inputDash = SystemAPI.GetSingleton<InputDashComponent>().Value;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().Value;
            uint inputDeploy = SystemAPI.GetSingleton<InputDeployComponent>().Value;
            int level1 = SystemAPI.GetSingleton<Level1EnergyForTutorialComponent>().Value;
            int level2 = SystemAPI.GetSingleton<Level2EnergyForTutorialComponent>().Value;
            int level3 = SystemAPI.GetSingleton<Level3EnergyForTutorialComponent>().Value;
            int maxTutorialLevelComponent = SystemAPI.GetSingleton<MaxLevelsForTutorialsComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;

            bool levelAdvanced = !_advanceLevelQuery.IsEmpty;
            int isLevelAdvanced = math.select(noAction , doAction , levelAdvanced);
            int evaluatedLevel = currentLevel + math.select(noAction , doAction , levelAdvanced);
            int isLevel1 = math.select(noAction , doAction , evaluatedLevel == doAction);
            int isLevel2 = math.select(noAction , doAction , evaluatedLevel == 2);
            int isLevel3 = math.select(noAction , doAction , evaluatedLevel >= 3);
            int minEnergy = isLevel1 * level1 + isLevel2 * level2 + isLevel3 * level3;

            uint playerInput = SystemAPI.GetSingleton<PlayerInputComponent>().Value;
            bool hasTurretSelected = SystemAPI.GetSingleton<SelectedTurretEntityComponent>().Entity != Entity.Null;
            bool anyKeyPressed = playerInput != inputNone;
            bool dashKeyPressed = (playerInput & inputDash) != inputNone;
            bool deployPressed = ((playerInput & inputDeploy) != inputNone) & hasTurretSelected;

            bool shouldDisableTurretsTutorial = (evaluatedLevel < maxTutorialLevelComponent & deployPressed) | (evaluatedLevel >= maxTutorialLevelComponent & anyKeyPressed) | (evaluatedLevel > maxTutorialLevelComponent);
            bool shouldEnableTurretsTutorial = levelAdvanced & (evaluatedLevel <= maxTutorialLevelComponent);

            bool shouldDisableLootTutorial = dashKeyPressed;

            foreach(RefRW<CurrentEnergyComponent> energy in SystemAPI.Query<RefRW<CurrentEnergyComponent>>()) { energy.ValueRW.Value = math.select(energy.ValueRO.Value , minEnergy , isLevelAdvanced == doAction); }

            foreach(var (_ , entity) in SystemAPI.Query<LootTutorialActiveTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                bool currentlyActive = SystemAPI.IsComponentEnabled<LootTutorialActiveTag>(entity);
                bool finalState = currentlyActive & !shouldDisableLootTutorial;
                SystemAPI.SetComponentEnabled<LootTutorialActiveTag>(entity , finalState);
            }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                bool currentlyActive = SystemAPI.IsComponentEnabled<TurretsTutorialActiveTag>(entity);
                bool finalState = shouldEnableTurretsTutorial | (currentlyActive & !shouldDisableTurretsTutorial);
                SystemAPI.SetComponentEnabled<TurretsTutorialActiveTag>(entity , finalState);
            }
        }
    }
}