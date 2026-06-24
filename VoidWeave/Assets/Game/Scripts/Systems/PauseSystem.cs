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
        private EntityQuery _lootTutorialQuery;

        public void OnCreate(ref SystemState systemState)
        {
            _lootTutorialQuery = SystemAPI.QueryBuilder().WithAll<LootTutorialActiveTag>().Build();
            
            systemState.RequireForUpdate<DashKeyComponent>();
            
            systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            Key dashKey = SystemAPI.GetSingleton<DashKeyComponent>().Value;

            bool isPaused = !systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled;
            bool hasLootTag = !_lootTutorialQuery.IsEmpty;
            bool dashKeyPressed = Keyboard.current != null && Keyboard.current[dashKey].wasPressedThisFrame;

            int triggerLootResume = math.select(0 , 1 , isPaused & hasLootTag & dashKeyPressed & !_isManualPaused);
            
            for(int i = 0 ; i < triggerLootResume ; i++)
            {
                ecb.AddComponent<LootTutorialResumeTag>(ecb.CreateEntity());

                foreach(var (_ , activeEntity) in SystemAPI.Query<RefRO<LootTutorialActiveTag>>().WithEntityAccess()) { SystemAPI.SetComponentEnabled<LootTutorialActiveTag>(activeEntity , false); }
            }

            foreach((RefRO<LootTutorialPauseTag> _ , Entity entity) in SystemAPI.Query<RefRO<LootTutorialPauseTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);

                int executePause = math.select(0 , 1 , hasLootTag);

                for(int i = 0 ; i < executePause ; i++)
                {
                    systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false;
                    foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = true; }
                }
            }

            foreach((RefRO<PauseInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<PauseInputTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                _isManualPaused = true;
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false;
                foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = true; }
            }

            foreach((RefRO<LootTutorialResumeTag> _ , Entity entity) in SystemAPI.Query<RefRO<LootTutorialResumeTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = true;
                foreach(var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>()) { vfx.Value.pause = false; }
            }

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