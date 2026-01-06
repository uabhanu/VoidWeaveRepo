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
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<LevelComponent>();
        }

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

            bool beamTurretKeyPressed = (playerInputComponent.SelectedInput & 256) != 0;
            bool scatterTurretKeyPressed = (playerInputComponent.SelectedInput & 128) != 0;
            bool strikerTurretKeyPressed = (playerInputComponent.SelectedInput & 64) != 0;

            // Check Unlocks (Level 2 for Scatter, Level 3 for Beam)
            bool beamTurretUnlocked = beamTurretKeyPressed && CurrentLevel >= 3;
            bool scatterTurretUnlocked = scatterTurretKeyPressed && CurrentLevel >= 2;
            
            bool fallbackToStrikerTurret = (scatterTurretKeyPressed && !scatterTurretUnlocked) || (beamTurretKeyPressed && !beamTurretUnlocked);
            bool selectedTurret = strikerTurretKeyPressed || fallbackToStrikerTurret;

            // --- Select Entity (Using Ternary ? : instead of math.select) ---
            // Apply Striker selection (Explicit press OR Fallback)
            selectedTurretEntityComponent.Entity = selectedTurret ? strikerTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;

            // Apply Scatter selection (Only if valid)
            selectedTurretEntityComponent.Entity = scatterTurretUnlocked ? scatterTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;

            // Apply Beam selection (Only if valid)
            selectedTurretEntityComponent.Entity = beamTurretUnlocked ? beamTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;

            // --- Select Cost ---
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , beamTurretCostComponent.Cost , beamTurretUnlocked);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , scatterTurretCostComponent.Cost , scatterTurretUnlocked);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , strikerTurretCostComponent.Cost , selectedTurret);
        }
    }
}