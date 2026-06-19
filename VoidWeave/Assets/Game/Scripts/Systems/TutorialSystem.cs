namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(CampaignProgressionSystem))]
    public partial struct TutorialSystem : ISystem
    {
        private EntityQuery _advanceLevelQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<InputDashComponent>();
            systemState.RequireForUpdate<InputDeployComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<Level1EnergyForTutorialComponent>();
            systemState.RequireForUpdate<Level2EnergyForTutorialComponent>();
            systemState.RequireForUpdate<Level3EnergyForTutorialComponent>();
            systemState.RequireForUpdate<MaxLevelsForTutorialsComponent>();
            systemState.RequireForUpdate<PlayerInputComponent>();
            systemState.RequireForUpdate<SelectedTurretEntityComponent>();

            _advanceLevelQuery = SystemAPI.QueryBuilder().WithAll<AdvanceLevelEventTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int currentLevel = SystemAPI.GetSingleton<LevelComponent>().Value;
            uint inputDash = SystemAPI.GetSingleton<InputDashComponent>().Value;
            uint inputDeploy = SystemAPI.GetSingleton<InputDeployComponent>().Value;
            int level1 = SystemAPI.GetSingleton<Level1EnergyForTutorialComponent>().Value;
            int level2 = SystemAPI.GetSingleton<Level2EnergyForTutorialComponent>().Value;
            int level3 = SystemAPI.GetSingleton<Level3EnergyForTutorialComponent>().Value;
            int maxTutorialLevelComponent = SystemAPI.GetSingleton<MaxLevelsForTutorialsComponent>().Value;

            bool levelAdvanced = !_advanceLevelQuery.IsEmpty;
            int isLevelAdvanced = math.select(0 , 1 , levelAdvanced);
            int evaluatedLevel = currentLevel + math.select(0 , 1 , levelAdvanced);
            int isLevel1 = math.select(0 , 1 , evaluatedLevel == 1);
            int isLevel2 = math.select(0 , 1 , evaluatedLevel == 2);
            int isLevel3 = math.select(0 , 1 , evaluatedLevel >= 3);
            int minEnergy = isLevel1 * level1 + isLevel2 * level2 + isLevel3 * level3;

            uint playerInput = SystemAPI.GetSingleton<PlayerInputComponent>().Value;
            bool hasTurretSelected = SystemAPI.GetSingleton<SelectedTurretEntityComponent>().Entity != Entity.Null;
            bool anyKeyPressed = playerInput != 0;
            bool dashKeyPressed = (playerInput & inputDash) != 0;
            bool deployPressed = ((playerInput & inputDeploy) != 0) & hasTurretSelected;

            bool shouldDisableTurretsTutorial = (evaluatedLevel < maxTutorialLevelComponent & deployPressed) | (evaluatedLevel >= maxTutorialLevelComponent & anyKeyPressed) | (evaluatedLevel > maxTutorialLevelComponent);
            bool shouldEnableTurretsTutorial = levelAdvanced & (evaluatedLevel <= maxTutorialLevelComponent);

            bool shouldDisableLootTutorial = dashKeyPressed;

            foreach(RefRW<CurrentEnergyComponent> energy in SystemAPI.Query<RefRW<CurrentEnergyComponent>>()) { energy.ValueRW.Value = math.select(energy.ValueRO.Value , minEnergy , isLevelAdvanced == 1); }

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