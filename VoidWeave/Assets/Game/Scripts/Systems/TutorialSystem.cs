namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(LevelProgressionSystem))] // Evaluate before the advance event entity is destroyed
    public partial struct TutorialSystem : ISystem
    {
        private EntityQuery _turretQuery;
        private EntityQuery _advanceLevelQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<NoActionComponent>();

            _turretQuery = SystemAPI.QueryBuilder().WithAll<TurretTag>().Build();
            _advanceLevelQuery = SystemAPI.QueryBuilder().WithAll<AdvanceLevelEventTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            int currentLevel = SystemAPI.GetSingleton<LevelComponent>().Value;

            // Is there a turret deployed? (1 if empty/no turrets, 0 if a turret exists)
            int tutorialActiveByDeployment = math.select(noAction , doAction , _turretQuery.IsEmpty);

            // Did we just progress to a new level on this exact frame? (1 if event entity exists, 0 if not)
            int levelJustAdvanced = math.select(noAction , doAction , !_advanceLevelQuery.IsEmpty);

            // Is the upcoming tutorial state permitted by the level rules? (1 if currentLevel <= 3, 0 if level >= 4)
            int tutorialAllowedByLevel = math.select(doAction , noAction , currentLevel >= 4);

            // Force the state back to true if a level up occurs, otherwise tie it to turret presence
            int calculatedState = math.select(tutorialActiveByDeployment , doAction , levelJustAdvanced == doAction);

            // Hard lock to false if we hit Level 4+
            bool finalTutorialState = (calculatedState * tutorialAllowedByLevel) == doAction;

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess()) { SystemAPI.SetComponentEnabled<TutorialActiveTag>(entity , finalTutorialState); }
        }
    }
}