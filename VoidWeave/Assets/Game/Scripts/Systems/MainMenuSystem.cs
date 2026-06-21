namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct MainMenuSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<GameStateComponent>();
            systemState.RequireForUpdate<PlayingStateComponent>();
            
            systemState.RequireForUpdate<StartGameRequestTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            var gameStateComponentEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            var playingStateComponent = SystemAPI.GetSingleton<PlayingStateComponent>();
            var startGameRequestEntity = SystemAPI.GetSingletonEntity<StartGameRequestTag>();

            ecb.SetComponent(gameStateComponentEntity , new GameStateComponent { Value = playingStateComponent.Value });
            ecb.AddComponent<InitializeGameTag>(gameStateComponentEntity);
            ecb.DestroyEntity(startGameRequestEntity);
        }
    }
}