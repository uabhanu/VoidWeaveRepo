namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UpgradeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate<CurrentEnergyComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            
            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeStrikerTurretTag>>().WithEntityAccess())
            {
                var strikerTurretCost = SystemAPI.GetSingletonRW<StrikerTurretCostComponent>();
                var strikerTurretLevel = SystemAPI.GetSingletonRW<StrikerTurretLevelComponent>();

                ProcessUpgrade(ref strikerTurretCost.ValueRW.Cost , ref currentEnergyComponent , ref strikerTurretLevel.ValueRW.Level , entityCommandBuffer , entity);
            }
            
            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeScatterTurretTag>>().WithEntityAccess())
            {
                var scatterTurretCost = SystemAPI.GetSingletonRW<ScatterTurretCostComponent>();
                var scatterTurretLevel = SystemAPI.GetSingletonRW<ScatterTurretLevelComponent>();

                ProcessUpgrade(ref scatterTurretCost.ValueRW.Cost , ref currentEnergyComponent , ref scatterTurretLevel.ValueRW.Level , entityCommandBuffer , entity);
            }
            
            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeBeamTurretTag>>().WithEntityAccess())
            {
                var beamTurretCost = SystemAPI.GetSingletonRW<BeamTurretCostComponent>();
                var beamTurretLevel = SystemAPI.GetSingletonRW<BeamTurretLevelComponent>();

                ProcessUpgrade(ref beamTurretCost.ValueRW.Cost , ref currentEnergyComponent , ref beamTurretLevel.ValueRW.Level , entityCommandBuffer , entity);
            }
        }

        private void ProcessUpgrade(ref int cost , ref RefRW<CurrentEnergyComponent> currentEnergyComponent , ref int level , EntityCommandBuffer entityCommandBuffer , Entity tagEntity)
        {
            bool canAfford = currentEnergyComponent.ValueRO.Energy >= cost;

            int costToDeduct = math.select(0 , cost , canAfford);
            float costScalingFactor = math.select(1.0f , 1.5f , canAfford);
            int levelToAdd = math.select(0 , 1 , canAfford);

            level += levelToAdd;
            cost = (int)(cost * costScalingFactor);
            currentEnergyComponent.ValueRW.Energy -= costToDeduct;

            entityCommandBuffer.DestroyEntity(tagEntity);
        }
    }
}