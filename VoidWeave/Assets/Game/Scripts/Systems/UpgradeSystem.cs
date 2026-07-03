namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct UpgradeSystem : ISystem
    {
        private EntityQuery _beamConfigQuery;
        private EntityQuery _scatterConfigQuery;
        private EntityQuery _strikerConfigQuery;
        
        public void OnCreate(ref SystemState systemState)
        {
            _beamConfigQuery = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent>().Build();
            _scatterConfigQuery = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent>().Build();
            _strikerConfigQuery = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent>().Build();
            
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<MaxTurretLevelComponent>();
            systemState.RequireForUpdate<UpgradeCostBaseMultiplierComponent>();
            systemState.RequireForUpdate<UpgradeCostMultiplierComponent>();
        }
        
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            RefRW<CurrentEnergyComponent> currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            int maxTurretLevel = SystemAPI.GetSingleton<MaxTurretLevelComponent>().Value;
            float upgradeCostBaseMultiplier = SystemAPI.GetSingleton<UpgradeCostBaseMultiplierComponent>().Value;
            float upgradeCostMultiplier = SystemAPI.GetSingleton<UpgradeCostMultiplierComponent>().Value;

            foreach((RefRO<UpgradeStrikerTurretTag> _ , Entity entity) in SystemAPI.Query<RefRO<UpgradeStrikerTurretTag>>().WithEntityAccess())
            {
                Entity configEntity = _strikerConfigQuery.GetSingletonEntity();

                RefRW<TurretCostComponent> strikerTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                RefRW<TurretLevelComponent> strikerTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref strikerTurretCost.ValueRW.Value , ref currentEnergyComponent , ecb , entity ,ref strikerTurretLevel.ValueRW.Value , maxTurretLevel , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }

            foreach((RefRO<UpgradeScatterTurretTag> _ , Entity entity) in SystemAPI.Query<RefRO<UpgradeScatterTurretTag>>().WithEntityAccess())
            {
                Entity configEntity = _scatterConfigQuery.GetSingletonEntity();

                RefRW<TurretCostComponent> scatterTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                RefRW<TurretLevelComponent> scatterTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref scatterTurretCost.ValueRW.Value , ref currentEnergyComponent , ecb , entity ,ref scatterTurretLevel.ValueRW.Value , maxTurretLevel , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }

            foreach((RefRO<UpgradeBeamTurretTag> _ , Entity entity) in SystemAPI.Query<RefRO<UpgradeBeamTurretTag>>().WithEntityAccess())
            {
                Entity configEntity = _beamConfigQuery.GetSingletonEntity();

                RefRW<TurretCostComponent> beamTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                RefRW<TurretLevelComponent> beamTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref beamTurretCost.ValueRW.Value , ref currentEnergyComponent , ecb , entity , ref beamTurretLevel.ValueRW.Value , maxTurretLevel , upgradeCostBaseMultiplier , upgradeCostMultiplier);
            }
        }

        private void ProcessUpgrade(ref int cost , ref RefRW<CurrentEnergyComponent> currentEnergyComponent , EntityCommandBuffer ecb , Entity entity , ref int level , int maxTurretLevel , float upgradeCostBaseMultiplier , float upgradeCostMultiplier)
        {
            bool canAfford = currentEnergyComponent.ValueRO.Value >= cost;
            bool isNotMaxLevel = level < maxTurretLevel;
            bool canUpgrade = canAfford & isNotMaxLevel;

            float costScalingFactor = math.select(upgradeCostBaseMultiplier , upgradeCostMultiplier , canUpgrade);
            int costToDeduct = math.select(0 , cost , canUpgrade);
            int levelToAdd = math.select(0 , 1 , canUpgrade);

            level += levelToAdd;
            cost = (int)(cost * costScalingFactor);
            currentEnergyComponent.ValueRW.Value -= costToDeduct;

            ecb.DestroyEntity(entity);
        }
    }
}