namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct UpgradeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<MaxTurretLevelComponent>();
            systemState.RequireForUpdate<UpgradeCostBaseMultiplierComponent>();
            systemState.RequireForUpdate<UpgradeCostMultiplierComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            RefRW<CurrentEnergyComponent> currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            int maxTurretLevel = SystemAPI.GetSingleton<MaxTurretLevelComponent>().Value;
            float upgradeCostBaseMultiplier = SystemAPI.GetSingleton<UpgradeCostBaseMultiplierComponent>().Value;
            float upgradeCostMultiplier = SystemAPI.GetSingleton<UpgradeCostMultiplierComponent>().Value;

            foreach((RefRO<UpgradeStrikerTurretTag> _ , Entity entity) in SystemAPI.Query<RefRO<UpgradeStrikerTurretTag>>().WithEntityAccess())
            {
                Entity configEntity = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                RefRW<TurretCostComponent> strikerTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                RefRW<TurretLevelComponent> strikerTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref strikerTurretCost.ValueRW.Value , ref currentEnergyComponent , ref strikerTurretLevel.ValueRW.Value , entityCommandBuffer , entity , 1 , maxTurretLevel , 0 , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }

            foreach((RefRO<UpgradeScatterTurretTag> _ , Entity entity) in SystemAPI.Query<RefRO<UpgradeScatterTurretTag>>().WithEntityAccess())
            {
                Entity configEntity = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                RefRW<TurretCostComponent> scatterTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                RefRW<TurretLevelComponent> scatterTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref scatterTurretCost.ValueRW.Value , ref currentEnergyComponent , ref scatterTurretLevel.ValueRW.Value , entityCommandBuffer , entity , 1 , maxTurretLevel , 0 , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }

            foreach((RefRO<UpgradeBeamTurretTag> _ , Entity entity) in SystemAPI.Query<RefRO<UpgradeBeamTurretTag>>().WithEntityAccess())
            {
                Entity configEntity = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                RefRW<TurretCostComponent> beamTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                RefRW<TurretLevelComponent> beamTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref beamTurretCost.ValueRW.Value , ref currentEnergyComponent , ref beamTurretLevel.ValueRW.Value , entityCommandBuffer , entity , 1 , maxTurretLevel , 0 , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }
        }

        private void ProcessUpgrade(ref int cost , ref RefRW<CurrentEnergyComponent> currentEnergyComponent , ref int level , EntityCommandBuffer entityCommandBuffer , Entity tagEntity , int doAction , int maxTurretLevel , int noAction , float upgradeCostBaseMultiplier , float upgradeCostMultiplier)
        {
            bool canAfford = currentEnergyComponent.ValueRO.Value >= cost;
            bool isNotMaxLevel = level < maxTurretLevel;
            bool canUpgrade = canAfford & isNotMaxLevel;

            float costScalingFactor = math.select(upgradeCostBaseMultiplier , upgradeCostMultiplier , canUpgrade);
            int costToDeduct = math.select(noAction , cost , canUpgrade);
            int levelToAdd = math.select(noAction , doAction , canUpgrade);

            level += levelToAdd;
            cost = (int)(cost * costScalingFactor);
            currentEnergyComponent.ValueRW.Value -= costToDeduct;

            entityCommandBuffer.DestroyEntity(tagEntity);
        }
    }
}