namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GameTestingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<CurrentEnergyComponent>();
            systemState.RequireForUpdate<CurrentEnergyWhileTestingComponent>();
            systemState.RequireForUpdate<IsTestingComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var isTestingComponent = SystemAPI.GetSingleton<IsTestingComponent>().Value;
            var currentEnergyComponent = SystemAPI.GetSingletonRW<CurrentEnergyComponent>();
            int currentEnergyWhileTestingComponent = SystemAPI.GetSingleton<CurrentEnergyWhileTestingComponent>().Value;
            
            currentEnergyComponent.ValueRW.Value = math.select(currentEnergyComponent.ValueRO.Value , currentEnergyWhileTestingComponent , isTestingComponent);
        }
    }
}