namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;
    using UnityEngine.VFX;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PauseSystem : ISystem
    {
        private bool _isManualPaused;

        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            systemState.RequireForUpdate<DoActionComponent>();
            systemState.RequireForUpdate<NoActionComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            
            bool isPaused = !systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled;
            bool hasLootTag = !SystemAPI.QueryBuilder().WithAll<LootTutorialActiveTag>().Build().IsEmpty;
            bool anyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

            // Any Key Generator: ONLY trigger if NOT manually paused
            int triggerLootResume = math.select(noAction , doAction , isPaused & hasLootTag & anyKeyPressed & !_isManualPaused);
            for(int i = noAction ; i < triggerLootResume ; i++) ecb.AddComponent<LootTutorialResumeTag>(ecb.CreateEntity());

            // Loot Pause
            foreach((RefRO<LootTutorialPauseTag> _ , Entity entity) in SystemAPI.Query<RefRO<LootTutorialPauseTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false;
                foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = true; }
            }

            // Manual Pause
            foreach((RefRO<PauseInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<PauseInputTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                _isManualPaused = true;
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false;
                foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = true; }
            }

            // Loot Resume
            foreach((RefRO<LootTutorialResumeTag> _ , Entity entity) in SystemAPI.Query<RefRO<LootTutorialResumeTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = true;
                foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = false; }
            }

            // Manual Resume
            foreach((RefRO<ResumeInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<ResumeInputTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                _isManualPaused = false;
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = true;
                foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = false; }
            }
        }
    }
}