namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct TurretDeploymentSystem : ISystem
    {
        private EntityQuery _turretQuery;

        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<InputDeployComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<NoActionComponent>();

            _turretQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<CollisionRadiusComponent , LocalTransform , TurretTag>().Build(ref systemState);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer.ParallelWriter ecbParallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged).AsParallelWriter();

            int currentEnergy = SystemAPI.GetSingleton<CurrentEnergyComponent>().Value;
            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            uint inputDeploy = SystemAPI.GetSingleton<InputDeployComponent>().Value;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;

            var energyNativeReference = new NativeReference<int>(currentEnergy , Allocator.TempJob);
            var existingPositionsNativeArray = _turretQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var existingRadiiNativeArray = _turretQuery.ToComponentDataArray<CollisionRadiusComponent>(Allocator.TempJob);

            var job = new TurretDeploymentJob { DoAction = doAction , EnergyNativeReference = energyNativeReference , EntityCommandBuffer = ecbParallelWriter , ExistingPositionsNativeArray = existingPositionsNativeArray , ExistingRadiiNativeArray = existingRadiiNativeArray , InputDeploy = inputDeploy , InputNone = inputNone , NoAction = noAction };

            job.Schedule(systemState.Dependency).Complete();

            SystemAPI.SetSingleton(new CurrentEnergyComponent { Value = energyNativeReference.Value });
            energyNativeReference.Dispose();
            existingPositionsNativeArray.Dispose();
            existingRadiiNativeArray.Dispose();
        }
    }

    [BurstCompile]
    public partial struct TurretDeploymentJob : IJobEntity
    {
        public int DoAction;
        public NativeReference<int> EnergyNativeReference;
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        [ReadOnly] public NativeArray<LocalTransform> ExistingPositionsNativeArray;
        [ReadOnly] public NativeArray<CollisionRadiusComponent> ExistingRadiiNativeArray;
        public uint InputDeploy;
        public uint InputNone;
        public int NoAction;

        private void Execute(in CollisionRadiusComponent collisionRadiusComponent , [EntityIndexInQuery] int entityInQueryIndex , in LocalTransform localTransform , in PlayerInputComponent playerInputComponent , in SelectedTurretCostComponent selectedTurretCostComponent , in SelectedTurretEntityComponent selectedTurretEntityComponent)
        {
            float isPositionValid = DoAction;

            for(int i = 0 ; i < ExistingPositionsNativeArray.Length ; i++)
            {
                float combinedRadius = collisionRadiusComponent.Value + ExistingRadiiNativeArray[i].Value;
                float minDistSq = combinedRadius * combinedRadius;
                float distSq = math.distancesq(localTransform.Position.xy , ExistingPositionsNativeArray[i].Position.xy);

                isPositionValid = math.select(isPositionValid , NoAction , distSq < minDistSq);
            }

            bool canAfford = EnergyNativeReference.Value >= selectedTurretCostComponent.Value;
            bool hasValidTurret = selectedTurretEntityComponent.Entity != Entity.Null;
            bool isDeployAction = (playerInputComponent.Value & InputDeploy) != InputNone;
            bool isOkToDeploy = (int)isPositionValid == DoAction;

            int spawnCount = math.select(NoAction , DoAction , isDeployAction && canAfford && hasValidTurret && isOkToDeploy);

            for(var i = 0 ; i < spawnCount ; i++)
            {
                Entity newTurret = EntityCommandBuffer.Instantiate(entityInQueryIndex , selectedTurretEntityComponent.Entity);
                EntityCommandBuffer.SetComponent(entityInQueryIndex , newTurret , LocalTransform.FromPosition(localTransform.Position));
            }

            EnergyNativeReference.Value -= selectedTurretCostComponent.Value * spawnCount;
        }
    }
}