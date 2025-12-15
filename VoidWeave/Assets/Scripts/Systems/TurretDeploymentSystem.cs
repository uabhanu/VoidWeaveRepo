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
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new TurretDeploymentJob { EntityCommandBuffer = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct TurretDeploymentJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

        private void Execute(ref CurrentEnergyComponent currentEnergyComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in SelectedTurretCostComponent selectedTurretCostComponent , in SelectedTurretEntityComponent selectedTurretEntityComponent , in TurretDeploymentInputComponent turretDeploymentInputComponent)
        {
            float isPressed = turretDeploymentInputComponent.IsPressed;
            int cost = selectedTurretCostComponent.Cost;
            Entity prefabToSpawn = selectedTurretEntityComponent.Entity;

            // Calculate Conditions
            bool hasEnoughEnergy = currentEnergyComponent.Energy >= cost;
            bool isValidPrefab = prefabToSpawn != Entity.Null;
            bool conditionsMet = (isPressed > 0.5f) && hasEnoughEnergy && isValidPrefab;

            // Determine Count (1 if valid, 0 if not)
            int deployCount = (int)math.select(0f , 1f , conditionsMet);
            
            // If count is 0, we subtract 0. If count is 1, we subtract cost.
            currentEnergyComponent.Energy -= cost * deployCount;
            
            for(int i = 0 ; i < deployCount ; i++)
            {
                Entity newTurret = EntityCommandBuffer.Instantiate(entityInQueryIndex , prefabToSpawn);
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newTurret , LocalTransform.FromPosition(localTransform.Position));
            }
        }
    }
}