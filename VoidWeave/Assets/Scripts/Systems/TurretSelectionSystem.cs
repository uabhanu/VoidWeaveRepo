namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TurretSelectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeamTurretUnlockLevelComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<InputTurret1Component>();
            systemState.RequireForUpdate<InputTurret2Component>();
            systemState.RequireForUpdate<InputTurret3Component>();
            systemState.RequireForUpdate<LevelComponent>();
            systemState.RequireForUpdate<ScatterTurretUnlockLevelComponent>();

            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent , TurretEntityComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent , TurretEntityComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent , TurretEntityComponent>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var strikerTurretQuery = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretEntityComponent>().Build();
            var scatterTurretQuery = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretEntityComponent>().Build();
            var beamTurretQuery = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretEntityComponent>().Build();

            var strikerTurretConfigEntity = strikerTurretQuery.GetSingletonEntity();
            var scatterTurretConfigEntity = scatterTurretQuery.GetSingletonEntity();
            var beamTurretConfigEntity = beamTurretQuery.GetSingletonEntity();

            Entity strikerTurretEntity = SystemAPI.GetComponent<TurretEntityComponent>(strikerTurretConfigEntity).Entity;
            Entity scatterTurretEntity = SystemAPI.GetComponent<TurretEntityComponent>(scatterTurretConfigEntity).Entity;
            Entity beamTurretEntity = SystemAPI.GetComponent<TurretEntityComponent>(beamTurretConfigEntity).Entity;

            int strikerTurretCost = SystemAPI.GetComponent<TurretCostComponent>(strikerTurretConfigEntity).Cost;
            int scatterTurretCost = SystemAPI.GetComponent<TurretCostComponent>(scatterTurretConfigEntity).Cost;
            int beamTurretCost = SystemAPI.GetComponent<TurretCostComponent>(beamTurretConfigEntity).Cost;

            int beamTurretUnlockLevel = SystemAPI.GetSingleton<BeamTurretUnlockLevelComponent>().Level;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().InputNone;
            uint inputTurret1 = SystemAPI.GetSingleton<InputTurret1Component>().InputTurret1;
            uint inputTurret2 = SystemAPI.GetSingleton<InputTurret2Component>().InputTurret2;
            uint inputTurret3 = SystemAPI.GetSingleton<InputTurret3Component>().InputTurret3;
            int scatterTurretUnlockLevel = SystemAPI.GetSingleton<ScatterTurretUnlockLevelComponent>().Level;

            new TurretSelectionJob
            {
                BeamTurretCost = beamTurretCost ,
                BeamTurretEntity = beamTurretEntity ,
                BeamTurretUnlockLevel = beamTurretUnlockLevel ,
                CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Level ,
                InputNone = inputNone ,
                InputTurret1 = inputTurret1 ,
                InputTurret2 = inputTurret2 ,
                InputTurret3 = inputTurret3 ,
                ScatterTurretCost = scatterTurretCost ,
                ScatterTurretEntity = scatterTurretEntity ,
                ScatterTurretUnlockLevel = scatterTurretUnlockLevel ,
                StrikerTurretCost = strikerTurretCost ,
                StrikerTurretEntity = strikerTurretEntity
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
        public uint InputNone;
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
            // Replaced 0 with InputNone
            bool strikerTurretKeyPressed = (playerInputComponent.PlayerInput & InputTurret1) != InputNone;
            bool scatterTurretKeyPressed = (playerInputComponent.PlayerInput & InputTurret2) != InputNone;
            bool beamTurretKeyPressed = (playerInputComponent.PlayerInput & InputTurret3) != InputNone;

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
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , StrikerTurretCost , strikerTurretKeyPressed);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , ScatterTurretCost , scatterTurretUnlocked);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , BeamTurretCost , beamTurretUnlocked);
        }
    }
}