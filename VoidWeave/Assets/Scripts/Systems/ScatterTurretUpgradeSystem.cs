namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ScatterTurretUpgradeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<BeginSimulationEntityCommandBufferSystem.Singleton>().Build());
            
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<CurrentEnergyComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ScatterTurretLevelComponent , ScatterTurretCostComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<UpgradeScatterTurretTag>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            var scatterTurretLevelComponent = SystemAPI.GetSingletonRW<ScatterTurretLevelComponent>();
            var scatterTurretCostComponent = SystemAPI.GetSingletonRW<ScatterTurretCostComponent>();

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeScatterTurretTag>>().WithEntityAccess())
            {
                bool canAfford = currentEnergyComponent.ValueRO.Energy >= scatterTurretCostComponent.ValueRO.Cost;

                int costToDeduct = math.select(0 , scatterTurretCostComponent.ValueRO.Cost , canAfford);
                float costScalingFactor = math.select(1.0f , 1.5f , canAfford);
                int levelToAdd = math.select(0 , 1 , canAfford);

                currentEnergyComponent.ValueRW.Energy -= costToDeduct;
                
                scatterTurretLevelComponent.ValueRW.Level += levelToAdd;
                scatterTurretCostComponent.ValueRW.Cost = (int)(scatterTurretCostComponent.ValueRO.Cost * costScalingFactor);

                ecb.DestroyEntity(entity);
            }
        }
    }
}