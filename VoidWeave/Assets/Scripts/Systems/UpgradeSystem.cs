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
                var configEntity = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                var strikerTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                var strikerTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref strikerTurretCost.ValueRW.Cost , ref currentEnergyComponent , ref strikerTurretLevel.ValueRW.Level , entityCommandBuffer , entity);
            }
            
            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeScatterTurretTag>>().WithEntityAccess())
            {
                var configEntity = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                var scatterTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                var scatterTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

                ProcessUpgrade(ref scatterTurretCost.ValueRW.Cost , ref currentEnergyComponent , ref scatterTurretLevel.ValueRW.Level , entityCommandBuffer , entity);
            }
            
            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeBeamTurretTag>>().WithEntityAccess())
            {
                var configEntity = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretCostComponent>().Build().GetSingletonEntity();

                var beamTurretCost = SystemAPI.GetComponentRW<TurretCostComponent>(configEntity);
                var beamTurretLevel = SystemAPI.GetComponentRW<TurretLevelComponent>(configEntity);

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