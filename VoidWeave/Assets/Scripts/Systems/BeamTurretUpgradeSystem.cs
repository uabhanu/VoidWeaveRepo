namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BeamTurretUpgradeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<BeginSimulationEntityCommandBufferSystem.Singleton>().Build());

            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<BeamTurretLevelComponent , BeamTurretCostComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<CurrentEnergyComponent>().Build());
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<UpgradeBeamTurretTag>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            
            var beamTurretLevelComponent = SystemAPI.GetSingletonRW<BeamTurretLevelComponent>();
            var beamTurretCostComponent = SystemAPI.GetSingletonRW<BeamTurretCostComponent>();
            
            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<UpgradeBeamTurretTag>>().WithEntityAccess())
            {
                bool canAfford = currentEnergyComponent.ValueRO.Energy >= beamTurretCostComponent.ValueRO.Cost;

                int costToDeduct = math.select(0 , beamTurretCostComponent.ValueRO.Cost , canAfford);
                float costScalingFactor = math.select(1.0f , 1.5f , canAfford);
                int levelToAdd = math.select(0 , 1 , canAfford);
                
                beamTurretLevelComponent.ValueRW.Level += levelToAdd;
                beamTurretCostComponent.ValueRW.Cost = (int)(beamTurretCostComponent.ValueRO.Cost * costScalingFactor);
                currentEnergyComponent.ValueRW.Energy -= costToDeduct;

                ecb.DestroyEntity(entity);
            }
        }
    }
}