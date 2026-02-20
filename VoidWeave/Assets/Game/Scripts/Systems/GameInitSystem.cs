namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GameInitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerEntityComponent , GameManagerEntityComponent , InitializeGameTag , PlayerEntityComponent , TurretConfigEntityComponent>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            Entity entity = SystemAPI.GetSingletonEntity<InitializeGameTag>();
            EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            var enemySpawnerEntityComponent = SystemAPI.GetComponent<EnemySpawnerEntityComponent>(entity);
            var gameManagerEntityComponent = SystemAPI.GetComponent<GameManagerEntityComponent>(entity);
            var inputEntityComponent = SystemAPI.GetComponent<InputEntityComponent>(entity);
            var playerEntityComponent = SystemAPI.GetComponent<PlayerEntityComponent>(entity);
            var turretConfigEntityComponent = SystemAPI.GetComponent<TurretConfigEntityComponent>(entity);

            entityCommandBuffer.Instantiate(enemySpawnerEntityComponent.Entity);
            entityCommandBuffer.Instantiate(gameManagerEntityComponent.Entity);
            entityCommandBuffer.Instantiate(inputEntityComponent.Entity);
            entityCommandBuffer.Instantiate(playerEntityComponent.Entity);
            entityCommandBuffer.Instantiate(turretConfigEntityComponent.Entity);

            entityCommandBuffer.RemoveComponent<InitializeGameTag>(entity);
        }
    }
}