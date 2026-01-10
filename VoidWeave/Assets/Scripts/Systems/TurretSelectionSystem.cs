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
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<LevelComponent>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { new TurretSelectionJob { CurrentLevel = SystemAPI.GetSingleton<LevelComponent>().Level }.ScheduleParallel(); }
    }

    [BurstCompile]
    public partial struct TurretSelectionJob : IJobEntity
    {
        public int CurrentLevel;

        private void Execute(in BeamTurretCostComponent beamTurretCostComponent , in BeamTurretEntityComponent beamTurretEntityComponent , in PlayerInputComponent playerInputComponent , in ScatterTurretCostComponent scatterTurretCostComponent , in ScatterTurretEntityComponent scatterTurretEntityComponent , ref SelectedTurretCostComponent selectedTurretCostComponent , ref SelectedTurretEntityComponent selectedTurretEntityComponent , in StrikerTurretCostComponent strikerTurretCostComponent , in StrikerTurretEntityComponent strikerTurretEntityComponent)
        {
            // Input Flags:
            // 64  = Key 1 (Striker)
            // 128 = Key 2 (Scatter)
            // 256 = Key 3 (Beam)

            bool strikerTurretKeyPressed = (playerInputComponent.SelectedInput & 64) != 0;
            bool scatterTurretKeyPressed = (playerInputComponent.SelectedInput & 128) != 0;
            bool beamTurretKeyPressed = (playerInputComponent.SelectedInput & 256) != 0;

            // Check Unlocks (Level 2 for Scatter, Level 3 for Beam)
            bool scatterTurretUnlocked = scatterTurretKeyPressed && CurrentLevel >= 2;
            bool beamTurretUnlocked = beamTurretKeyPressed && CurrentLevel >= 3;

            // --- Select Entity ---
            // Striker (Base priority): Set if pressed.
            selectedTurretEntityComponent.Entity = strikerTurretKeyPressed ? strikerTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;

            // Scatter: If pressed -> (Unlocked ? Scatter : Null). Else keep current.
            // This ensures pressing a locked key clears the selection instead of keeping the previous one.
            selectedTurretEntityComponent.Entity = scatterTurretKeyPressed ? scatterTurretUnlocked ? scatterTurretEntityComponent.Entity : Entity.Null : selectedTurretEntityComponent.Entity;

            // Beam: If pressed -> (Unlocked ? Beam : Null). Else keep current.
            selectedTurretEntityComponent.Entity = beamTurretKeyPressed ? beamTurretUnlocked ? beamTurretEntityComponent.Entity : Entity.Null : selectedTurretEntityComponent.Entity;

            // Must match Entity order exactly (Striker -> Scatter -> Beam)
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , strikerTurretCostComponent.Cost , strikerTurretKeyPressed);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , scatterTurretCostComponent.Cost , scatterTurretUnlocked);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , beamTurretCostComponent.Cost , beamTurretUnlocked);
        }
    }
}