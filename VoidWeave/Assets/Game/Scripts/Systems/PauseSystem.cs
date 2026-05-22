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
            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach((RefRO<PauseInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<PauseInputTag>>().WithEntityAccess())
            {
                ecb.AddComponent<GamePausedTag>(entity);
                ecb.DestroyEntity(entity);
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false;
            }

            foreach((RefRO<ResumeInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<ResumeInputTag>>().WithEntityAccess())
            {
                ecb.RemoveComponent<GamePausedTag>(entity);
                ecb.DestroyEntity(entity);
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = true;
            }
        }
    }
}