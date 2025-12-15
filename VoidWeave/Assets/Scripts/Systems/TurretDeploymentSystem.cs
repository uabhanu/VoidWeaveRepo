namespace Systems
{
    using Gameplay;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TurretDeploymentSystem : ISystem
    {
        private const float DEPLOY_OFFSET_X = 1.5f;
        private const int STRIKER_COST = 100;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<TurretEntityComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var strikerPrefab = SystemAPI.GetSingleton<TurretEntityComponent>().TurretEntity;

            new DeploymentJob { DeployOffset = new float3(DEPLOY_OFFSET_X , 0 , 0) , EntityCommandBuffer = ecb , StrikerCost = STRIKER_COST , StrikerPrefab = strikerPrefab }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct DeploymentJob : IJobEntity
    {
        public float3 DeployOffset;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public int StrikerCost;
        public Entity StrikerPrefab;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , ref CurrentEnergyComponent currentEnergyComponent , ref TurretDeploymentInputComponent turretDeploymentInputComponent , in LocalTransform localTransform)
        {
            float canAfford = math.step((float)StrikerCost , (float)currentEnergyComponent.Energy);
            float isInputPressed = math.step(0.5f , turretDeploymentInputComponent.IsPressed);
            
            float shouldDeploy = isInputPressed * canAfford;
            
            int spawnCount = (int)shouldDeploy;
            
            currentEnergyComponent.Energy -= (StrikerCost * spawnCount);
            
            turretDeploymentInputComponent.IsPressed *= (1 - spawnCount);
            
            for(int i = 0 ; i < spawnCount ; i++)
            {
                Entity newTurret = EntityCommandBuffer.Instantiate(entityInQueryIndex , StrikerPrefab);
                float3 spawnPos = localTransform.Position + DeployOffset;

                EntityCommandBuffer.SetComponent(entityInQueryIndex , newTurret , LocalTransform.FromPosition(spawnPos));
            }
        }
    }
}