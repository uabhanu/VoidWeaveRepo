namespace Game.Scripts.Systems
{
    using Game.Scripts.Components;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine.SceneManagement;

    public partial class RestartSystem : SystemBase
    {
        protected override void OnCreate() { RequireForUpdate<RestartTag>(); }

        protected override void OnUpdate()
        {
            // We destroy the entity so this system doesn't run again next frame to prevent infinite restarts
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach((RefRO<RestartTag> _ , Entity entity) in SystemAPI.Query<RefRO<RestartTag>>().WithEntityAccess()) ecb.DestroyEntity(entity);

            ecb.Playback(EntityManager);
            ecb.Dispose();

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}