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
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<InputDeployComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            int currentEnergy = SystemAPI.GetSingleton<CurrentEnergyComponent>().Energy;
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().DoAction;
            uint inputDeploy = SystemAPI.GetSingleton<InputDeployComponent>().InputDeploy;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().InputNone;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().NoActionValue;

            NativeReference<int> energyRef = new NativeReference<int>(currentEnergy , Allocator.TempJob);

            var job = new TurretDeploymentJob { DoAction = doAction , EnergyRef = energyRef , EntityCommandBuffer = ecbParallelWriter , InputDeploy = inputDeploy , InputNone = inputNone , NoAction = noAction };

            job.Schedule(systemState.Dependency).Complete();

            SystemAPI.SetSingleton(new CurrentEnergyComponent { Energy = energyRef.Value });
            energyRef.Dispose();
        }
    }

    [BurstCompile]
    public partial struct TurretDeploymentJob : IJobEntity
    {
        public int DoAction;
        public NativeReference<int> EnergyRef;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        public uint InputDeploy;
        public uint InputNone;
        public int NoAction;

        private void Execute([EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in PlayerInputComponent playerInputComponent , in SelectedTurretCostComponent selectedTurretCostComponent , in SelectedTurretEntityComponent selectedTurretEntityComponent)
        {
            bool canAfford = EnergyRef.Value >= selectedTurretCostComponent.Cost;
            bool hasValidTurret = selectedTurretEntityComponent.Entity != Entity.Null;
            bool isDeployAction = (playerInputComponent.PlayerInput & InputDeploy) != InputNone;
            
            int spawnCount = math.select(NoAction , DoAction , isDeployAction && canAfford && hasValidTurret);

            for(int i = 0 ; i < spawnCount ; i++)
            {
                Entity newTurret = EntityCommandBuffer.Instantiate(entityInQueryIndex , selectedTurretEntityComponent.Entity);
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newTurret , LocalTransform.FromPosition(localTransform.Position));
            }

            EnergyRef.Value -= selectedTurretCostComponent.Cost * spawnCount;
        }
    }
}