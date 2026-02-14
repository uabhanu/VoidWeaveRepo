namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PauseSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<GameManagerEntityComponent>();
            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            Entity gameManagerEntityComponent = SystemAPI.GetSingletonEntity<GameManagerEntityComponent>();

            foreach((RefRO<PauseInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<PauseInputTag>>().WithEntityAccess())
            {
                ecb.AddComponent<GamePausedTag>(gameManagerEntityComponent);
                ecb.DestroyEntity(entity);
                Time.timeScale = 0;
            }

            foreach((RefRO<ResumeInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<ResumeInputTag>>().WithEntityAccess())
            {
                ecb.RemoveComponent<GamePausedTag>(gameManagerEntityComponent);
                ecb.DestroyEntity(entity);
                Time.timeScale = 1;
            }
        }
    }
}