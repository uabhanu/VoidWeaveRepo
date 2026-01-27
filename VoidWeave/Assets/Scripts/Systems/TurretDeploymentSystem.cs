namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TurretDeploymentSystem : ISystem
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
            var ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();
            int currentEnergy = SystemAPI.GetSingleton<CurrentEnergyComponent>().Energy;
            NativeReference<int> energyRef = new NativeReference<int>(currentEnergy , Allocator.TempJob);

            var job = new TurretDeploymentJob { EntityCommandBuffer = ecbParallelWriter , EnergyRef = energyRef };
            job.Schedule(systemState.Dependency).Complete();
            
            SystemAPI.SetSingleton(new CurrentEnergyComponent { Energy = energyRef.Value });
            energyRef.Dispose();
        }
    }

    [BurstCompile]
    public partial struct TurretDeploymentJob : IJobEntity
    {
        public NativeReference<int> EnergyRef;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in PlayerInputComponent playerInputComponent , in SelectedTurretCostComponent selectedTurretCostComponent , in SelectedTurretEntityComponent selectedTurretEntityComponent)
        {
            bool canAfford = EnergyRef.Value >= selectedTurretCostComponent.Cost;
            bool isDeployAction = (playerInputComponent.PlayerInput & 32) != 0;
            bool hasValidTurret = selectedTurretEntityComponent.Entity != Entity.Null;

            int spawnCount = math.select(0 , 1 , isDeployAction && canAfford && hasValidTurret);

            for(int i = 0 ; i < spawnCount ; i++)
            {
                Entity newTurret = EntityCommandBuffer.Instantiate(entityInQueryIndex , selectedTurretEntityComponent.Entity);
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newTurret , LocalTransform.FromPosition(localTransform.Position));
            }
            
            EnergyRef.Value -= selectedTurretCostComponent.Cost * spawnCount;
        }
    }
}