namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(MainMenuSystem))]
    public partial struct GameInitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<GameBackgroundEntityComponent>();
            systemState.RequireForUpdate<InitializeGameTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var tagEntity = SystemAPI.GetSingletonEntity<InitializeGameTag>();
            var dataEntity = SystemAPI.GetSingletonEntity<GameBackgroundEntityComponent>();

            var entityCommandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            
            var enemySpawner = SystemAPI.GetComponent<EnemySpawnerEntityComponent>(dataEntity);
            var gameBackground = SystemAPI.GetComponent<GameBackgroundEntityComponent>(dataEntity);
            var input = SystemAPI.GetComponent<InputEntityComponent>(dataEntity);
            var player = SystemAPI.GetComponent<PlayerEntityComponent>(dataEntity);
            var turretConfig = SystemAPI.GetComponent<TurretConfigEntityComponent>(dataEntity);
            
            entityCommandBuffer.Instantiate(enemySpawner.Entity);
            entityCommandBuffer.Instantiate(gameBackground.Entity);
            entityCommandBuffer.Instantiate(input.Entity);
            entityCommandBuffer.Instantiate(player.Entity);
            entityCommandBuffer.Instantiate(turretConfig.Entity);
            
            entityCommandBuffer.RemoveComponent<InitializeGameTag>(tagEntity);
        }
    }
}