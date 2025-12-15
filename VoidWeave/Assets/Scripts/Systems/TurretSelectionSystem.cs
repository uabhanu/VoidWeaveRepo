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
            float press1 = strikerTurretInputComponent.Input;
            float press2 = scatterTurretInputComponent.Input;

            Entity currentPrefab = selectedTurretEntityComponent.Entity;
            currentPrefab = press1 > 0.5f ? strikerTurretEntityComponent.Entity : currentPrefab;
            currentPrefab = press2 > 0.5f ? scatterTurretEntityComponent.Entity : currentPrefab;
            selectedTurretEntityComponent.Entity = currentPrefab;

            int currentCost = selectedTurretCostComponent.Cost;
            currentCost = math.select(currentCost , strikerTurretCostComponent.Cost , press1 > 0.5f);
            currentCost = math.select(currentCost , scatterTurretCostComponent.Cost , press2 > 0.5f);
            selectedTurretCostComponent.Cost = currentCost;
        }
    }
}