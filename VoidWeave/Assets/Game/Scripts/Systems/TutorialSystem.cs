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
            systemState.RequireForUpdate<Level3EnergyForTutorialComponent>();
            systemState.RequireForUpdate<Level2EnergyForTutorialComponent>();
            systemState.RequireForUpdate<Level1EnergyForTutorialComponent>();
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<InputDeployComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<MaxLevelsForTutorialsComponent>();
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
            bool deployPressed = ((playerInput & inputDeploy) != inputNone) & hasTurretSelected;
            bool anyKeyPressed = playerInput != inputNone;

            bool shouldDisable = (evaluatedLevel < maxTutorialLevelComponent & deployPressed) | (evaluatedLevel >= maxTutorialLevelComponent & anyKeyPressed) | (evaluatedLevel > maxTutorialLevelComponent);
            bool shouldEnable = levelAdvanced & (evaluatedLevel <= maxTutorialLevelComponent);

            foreach(RefRW<CurrentEnergyComponent> energy in SystemAPI.Query<RefRW<CurrentEnergyComponent>>()) { energy.ValueRW.Value = math.select(energy.ValueRO.Value , minEnergy , isLevelAdvanced == doAction); }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                bool currentlyActive = SystemAPI.IsComponentEnabled<TutorialActiveTag>(entity);
                bool finalState = shouldEnable | (currentlyActive & !shouldDisable);
                SystemAPI.SetComponentEnabled<TutorialActiveTag>(entity , finalState);
            }
        }
    }
}