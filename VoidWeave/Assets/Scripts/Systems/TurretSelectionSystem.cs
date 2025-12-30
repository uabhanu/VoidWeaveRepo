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
        private void Execute(in PlayerInputComponent playerInputComponent , in ScatterTurretCostComponent scatterTurretCostComponent , in ScatterTurretEntityComponent scatterTurretEntityComponent , ref SelectedTurretCostComponent selectedTurretCostComponent , ref SelectedTurretEntityComponent selectedTurretEntityComponent , in StrikerTurretCostComponent strikerTurretCostComponent , in StrikerTurretEntityComponent strikerTurretEntityComponent)
        {
            // SelectedInput & 128 = Scatter (Key 2) , SelectedInput & 64 = Striker (Key 1)

            // Select Entity
            selectedTurretEntityComponent.Entity = (playerInputComponent.SelectedInput & 128) != 0 ? scatterTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;
            selectedTurretEntityComponent.Entity = (playerInputComponent.SelectedInput & 64) != 0 ? strikerTurretEntityComponent.Entity : selectedTurretEntityComponent.Entity;

            // Select Cost
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , scatterTurretCostComponent.Cost , (playerInputComponent.SelectedInput & 128) != 0);
            selectedTurretCostComponent.Cost = math.select(selectedTurretCostComponent.Cost , strikerTurretCostComponent.Cost , (playerInputComponent.SelectedInput & 64) != 0);
        }
    }
}