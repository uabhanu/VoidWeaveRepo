namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct StrikerTurretUpgradeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<BeginSimulationEntityCommandBufferSystem.Singleton>().Build());
            
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<CurrentEnergyComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<StrikerTurretLevelComponent , StrikerTurretCostComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<UpgradeStrikerTurretTag>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            
            var strikerTurretLevelComponent = SystemAPI.GetSingletonRW<StrikerTurretLevelComponent>();
            var strikerTurretCostComponent = SystemAPI.GetSingletonRW<StrikerTurretCostComponent>();

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeStrikerTurretTag>>().WithEntityAccess())
            {
                bool canAfford = currentEnergyComponent.ValueRO.Energy >= strikerTurretCostComponent.ValueRO.Cost;

                int costToDeduct = math.select(0 , strikerTurretCostComponent.ValueRO.Cost , canAfford);
                float costScalingFactor = math.select(1.0f , 1.5f , canAfford);
                int levelToAdd = math.select(0 , 1 , canAfford);

                strikerTurretLevelComponent.ValueRW.Level += levelToAdd;
                strikerTurretCostComponent.ValueRW.Cost = (int)(strikerTurretCostComponent.ValueRO.Cost * costScalingFactor);
                currentEnergyComponent.ValueRW.Energy -= costToDeduct;

                ecb.DestroyEntity(entity);
            }
        }
    }
}