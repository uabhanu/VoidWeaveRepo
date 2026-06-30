namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct TurretSelectionSystem : ISystem
    {
        private EntityQuery _beamQuery;
        private EntityQuery _scatterQuery;
        private EntityQuery _strikerQuery;
        
        public void OnCreate(ref SystemState systemState)
        {
            _beamQuery = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent , TurretEntityComponent>().Build();
            _scatterQuery = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent , TurretEntityComponent>().Build();
            _strikerQuery = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent , TurretEntityComponent>().Build();
            
            systemState.RequireForUpdate<BeamTurretUnlockLevelComponent>();
            systemState.RequireForUpdate<InputTurret1Component>();
            systemState.RequireForUpdate<InputTurret2Component>();
            systemState.RequireForUpdate<InputTurret3Component>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<ScatterTurretUnlockLevelComponent>();

            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent , TurretEntityComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent , TurretEntityComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent , TurretEntityComponent>().Build());

            systemState.RequireForUpdate(_beamQuery);
            systemState.RequireForUpdate(_scatterQuery);
            systemState.RequireForUpdate(_strikerQuery);
        }
        
        public void OnUpdate(ref SystemState systemState)
        {
            new TurretSelectionJob
            {
                BeamTurretCost = SystemAPI.GetComponent<TurretCostComponent>(_beamQuery.GetSingletonEntity()).Value ,
                BeamTurretEntity = SystemAPI.GetComponent<TurretEntityComponent>(_beamQuery.GetSingletonEntity()).Entity ,
                BeamTurretUnlockLevel = SystemAPI.GetSingleton<BeamTurretUnlockLevelComponent>().Value ,
                CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Value ,
                InputTurret1 = SystemAPI.GetSingleton<InputTurret1Component>().Value ,
                InputTurret2 = SystemAPI.GetSingleton<InputTurret2Component>().Value ,
                InputTurret3 = SystemAPI.GetSingleton<InputTurret3Component>().Value ,
                ScatterTurretCost = SystemAPI.GetComponent<TurretCostComponent>(_scatterQuery.GetSingletonEntity()).Value ,
                ScatterTurretEntity = SystemAPI.GetComponent<TurretEntityComponent>(_scatterQuery.GetSingletonEntity()).Entity ,
                ScatterTurretUnlockLevel = SystemAPI.GetSingleton<ScatterTurretUnlockLevelComponent>().Value ,
                StrikerTurretCost = SystemAPI.GetComponent<TurretCostComponent>(_strikerQuery.GetSingletonEntity()).Value ,
                StrikerTurretEntity = SystemAPI.GetComponent<TurretEntityComponent>(_strikerQuery.GetSingletonEntity()).Entity
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct TurretSelectionJob : IJobEntity
    {
        public int BeamTurretCost;
        public Entity BeamTurretEntity;
        public int BeamTurretUnlockLevel;
        public int CurrentLevel;
        public uint InputTurret1;
        public uint InputTurret2;
        public uint InputTurret3;
        public int ScatterTurretCost;
        public Entity ScatterTurretEntity;
        public int ScatterTurretUnlockLevel;
        public int StrikerTurretCost;
        public Entity StrikerTurretEntity;

        private void Execute(in PlayerInputComponent playerInputComponent , ref SelectedTurretCostComponent selectedTurretCostComponent , ref SelectedTurretEntityComponent selectedTurretEntityComponent)
        {
            // Input Flags: Replaced 64, 128, 256 with InputTurret Components
            // Replaced 0 with Value
            bool strikerTurretKeyPressed = (playerInputComponent.Value & InputTurret1) != 0;
            bool scatterTurretKeyPressed = (playerInputComponent.Value & InputTurret2) != 0;
            bool beamTurretKeyPressed = (playerInputComponent.Value & InputTurret3) != 0;

            // Check Unlocks: Replaced 2 and 3 with UnlockLevel Components
            bool scatterTurretUnlocked = scatterTurretKeyPressed && CurrentLevel >= ScatterTurretUnlockLevel;
            bool beamTurretUnlocked = beamTurretKeyPressed && CurrentLevel >= BeamTurretUnlockLevel;

            // --- Select Entity ---
            // Striker (Base priority): Set if pressed.
            selectedTurretEntityComponent.Entity = strikerTurretKeyPressed ? StrikerTurretEntity : selectedTurretEntityComponent.Entity;

            // Scatter: If pressed -> (Unlocked ? Scatter : Null). Else keep current.
            selectedTurretEntityComponent.Entity = scatterTurretKeyPressed ? scatterTurretUnlocked ? ScatterTurretEntity : Entity.Null : selectedTurretEntityComponent.Entity;

            // Beam: If pressed -> (Unlocked ? Beam : Null). Else keep current.
            selectedTurretEntityComponent.Entity = beamTurretKeyPressed ? beamTurretUnlocked ? BeamTurretEntity : Entity.Null : selectedTurretEntityComponent.Entity;

            // Must match Entity order exactly (Striker -> Scatter -> Beam)
            selectedTurretCostComponent.Value = math.select(selectedTurretCostComponent.Value , StrikerTurretCost , strikerTurretKeyPressed);
            selectedTurretCostComponent.Value = math.select(selectedTurretCostComponent.Value , ScatterTurretCost , scatterTurretUnlocked);
            selectedTurretCostComponent.Value = math.select(selectedTurretCostComponent.Value , BeamTurretCost , beamTurretUnlocked);
        }
    }
}