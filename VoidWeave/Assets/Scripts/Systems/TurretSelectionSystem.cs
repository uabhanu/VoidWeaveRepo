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
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new TurretSelectionJob().ScheduleParallel(); }
    }

    [BurstCompile]
    public partial struct TurretSelectionJob : IJobEntity
    {
        private void Execute(in ScatterTurretCostComponent scatterTurretCostComponent , in ScatterTurretEntityComponent scatterTurretEntityComponent , in ScatterTurretInputComponent scatterTurretInputComponent , ref SelectedTurretCostComponent selectedTurretCostComponent , ref SelectedTurretEntityComponent selectedTurretEntityComponent , in StrikerTurretCostComponent strikerTurretCostComponent , in StrikerTurretEntityComponent strikerTurretEntityComponent , in StrikerTurretInputComponent strikerTurretInputComponent)
        {
            // Default to current -> If Scatter Pressed, pick Scatter -> If Striker Pressed, pick Striker (Priority to Striker)
            // Select Entity (Ternary is required for Entity struct, but logic is branchless)
            selectedTurretEntityComponent.Entity = scatterTurretInputComponent.Input > 0.5f ? scatterTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;
            selectedTurretEntityComponent.Entity = strikerTurretInputComponent.Input > 0.5f ? strikerTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;
            
            // Select Cost (math.select is strictly branchless)
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , scatterTurretCostComponent.Cost , scatterTurretInputComponent.Input > 0.5f);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , strikerTurretCostComponent.Cost , strikerTurretInputComponent.Input > 0.5f);
        }
    }
}