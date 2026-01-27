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
            systemState.RequireForUpdate<LevelComponent>();

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

            new TurretSelectionJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Level , StrikerTurretEntity = strikerTurretEntity , StrikerTurretCost = strikerTurretCost , ScatterTurretEntity = scatterTurretEntity , ScatterTurretCost = scatterTurretCost , BeamTurretEntity = beamTurretEntity , BeamTurretCost = beamTurretCost }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct TurretSelectionJob : IJobEntity
    {
        public int CurrentLevel;

        public int BeamTurretCost;
        public Entity BeamTurretEntity;

        public int ScatterTurretCost;
        public Entity ScatterTurretEntity;

        public int StrikerTurretCost;
        public Entity StrikerTurretEntity;

        private void Execute(in PlayerInputComponent playerInputComponent , ref SelectedTurretCostComponent selectedTurretCostComponent , ref SelectedTurretEntityComponent selectedTurretEntityComponent)
        {
            // Input Flags:
            // 64  = Key 1 (Striker)
            // 128 = Key 2 (Scatter)
            // 256 = Key 3 (Beam)

            bool strikerTurretKeyPressed = (playerInputComponent.PlayerInput & 64) != 0;
            bool scatterTurretKeyPressed = (playerInputComponent.PlayerInput & 128) != 0;
            bool beamTurretKeyPressed = (playerInputComponent.PlayerInput & 256) != 0;

            // Check Unlocks (Level 2 for Scatter, Level 3 for Beam)
            bool scatterTurretUnlocked = scatterTurretKeyPressed && CurrentLevel >= 2;
            bool beamTurretUnlocked = beamTurretKeyPressed && CurrentLevel >= 3;

            // --- Select Entity ---
            // Striker (Base priority): Set if pressed.
            selectedTurretEntityComponent.Entity = strikerTurretKeyPressed ? StrikerTurretEntity : selectedTurretEntityComponent.Entity;

            // Scatter: If pressed -> (Unlocked ? Scatter : Null). Else keep current.
            // This ensures pressing a locked key clears the selection instead of keeping the previous one.
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