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
            var dataEntity = SystemAPI.GetSingletonEntity<GameBackgroundEntityComponent>();

            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            var enemySpawner = SystemAPI.GetComponent<EnemySpawnerEntityComponent>(dataEntity);
            var gameBackground = SystemAPI.GetComponent<GameBackgroundEntityComponent>(dataEntity);
            var input = SystemAPI.GetComponent<InputEntityComponent>(dataEntity);
            var player = SystemAPI.GetComponent<PlayerEntityComponent>(dataEntity);
            var turretConfig = SystemAPI.GetComponent<TurretConfigEntityComponent>(dataEntity);

            foreach(var (_ , tagEntity) in SystemAPI.Query<RefRO<InitializeGameTag>>().WithEntityAccess())
            {
                ecb.Instantiate(enemySpawner.Entity);
                ecb.Instantiate(gameBackground.Entity);
                ecb.Instantiate(input.Entity);
                ecb.Instantiate(player.Entity);
                ecb.Instantiate(turretConfig.Entity);
                
                ecb.SetComponentEnabled<InitializeGameTag>(tagEntity , false);
            }
        }
    }
}