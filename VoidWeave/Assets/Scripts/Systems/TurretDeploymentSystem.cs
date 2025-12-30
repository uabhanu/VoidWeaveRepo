namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TurretDeploymentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new TurretDeploymentJob { EntityCommandBuffer = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct TurretDeploymentJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        private void Execute(ref CurrentEnergyComponent currentEnergyComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in PlayerInputComponent playerInputComponent , in SelectedTurretCostComponent selectedTurretCostComponent , in SelectedTurretEntityComponent selectedTurretEntityComponent)
        {
            // SelectedInput & 32 corresponds to the Deploy input bit defined in InputSystem
            for(int i = 0 ; i < math.select(0 , 1 , ((playerInputComponent.SelectedInput & 32) != 0) && (currentEnergyComponent.Energy >= selectedTurretCostComponent.Cost) && (selectedTurretEntityComponent.Entity != Entity.Null)) ; i++)
            {
                Entity newTurret = EntityCommandBuffer.Instantiate(entityInQueryIndex , selectedTurretEntityComponent.Entity);
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newTurret , LocalTransform.FromPosition(localTransform.Position));
            }

            currentEnergyComponent.Energy -= selectedTurretCostComponent.Cost * math.select(0 , 1 , ((playerInputComponent.SelectedInput & 32) != 0) && (currentEnergyComponent.Energy >= selectedTurretCostComponent.Cost) && (selectedTurretEntityComponent.Entity != Entity.Null));
        }
    }
}