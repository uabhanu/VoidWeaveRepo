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
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<UpgradeCostBaseMultiplierComponent>();
            systemState.RequireForUpdate<UpgradeCostMultiplierComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float upgradeCostBaseMultiplier = SystemAPI.GetSingleton<UpgradeCostBaseMultiplierComponent>().Value;
            float upgradeCostMultiplier = SystemAPI.GetSingleton<UpgradeCostMultiplierComponent>().Value;

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeStrikerTurretTag>>().WithEntityAccess())
            {
                var configEntity = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                var strikerTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                var strikerTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref strikerTurretCost.ValueRW.Value , ref currentEnergyComponent , ref strikerTurretLevel.ValueRW.Value , entityCommandBuffer , entity , doAction , noAction , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeScatterTurretTag>>().WithEntityAccess())
            {
                var configEntity = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                var scatterTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                var scatterTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref scatterTurretCost.ValueRW.Value , ref currentEnergyComponent , ref scatterTurretLevel.ValueRW.Value , entityCommandBuffer , entity , doAction , noAction , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeBeamTurretTag>>().WithEntityAccess())
            {
                var configEntity = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                var beamTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                var beamTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref beamTurretCost.ValueRW.Value , ref currentEnergyComponent , ref beamTurretLevel.ValueRW.Value , entityCommandBuffer , entity , doAction , noAction , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }
        }

        private void ProcessUpgrade(ref int cost , ref RefRW<CurrentEnergyComponent> currentEnergyComponent , ref int level , EntityCommandBuffer entityCommandBuffer , Entity tagEntity , int doAction , int noAction , float upgradeCostBaseMultiplier , float upgradeCostMultiplier)
        {
            bool canAfford = currentEnergyComponent.ValueRO.Value >= cost;
            float costScalingFactor = math.select(upgradeCostBaseMultiplier , upgradeCostMultiplier , canAfford);
            int costToDeduct = math.select(noAction , cost , canAfford);
            int levelToAdd = math.select(noAction , doAction , canAfford);

            level += levelToAdd;
            cost = (int)(cost * costScalingFactor);
            currentEnergyComponent.ValueRW.Value -= costToDeduct;

            entityCommandBuffer.DestroyEntity(tagEntity);
        }
    }
}