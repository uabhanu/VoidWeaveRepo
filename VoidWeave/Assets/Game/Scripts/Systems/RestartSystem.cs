namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct RestartSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach((RefRO<RestartInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<RestartInputTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}