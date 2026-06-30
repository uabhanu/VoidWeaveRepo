namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.InputSystem;

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
                // ACCEPTABLE EXCEPTION: AddComponent is safe here because it is applied to a brand new ecb.CreateEntity() at birth. 
                // It does not force an existing entity to move memory chunks.
                ecb.AddComponent<LootTutorialResumeTag>(ecb.CreateEntity());
                foreach(var (_ , activeEntity) in SystemAPI.Query<RefRO<LootTutorialActiveTag>>().WithEntityAccess()) { SystemAPI.SetComponentEnabled<LootTutorialActiveTag>(activeEntity , false); }
            }

            foreach((RefRO<LootTutorialPauseTag> _ , Entity entity) in SystemAPI.Query<RefRO<LootTutorialPauseTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                int executePause = math.select(0 , 1 , hasLootTag);

                for(int i = 0 ; i < executePause ; i++)
                {
                    systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false; //ECS Pause
                    Time.timeScale = 0f; //Unity Pause
                }
            }

            foreach((RefRO<PauseInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<PauseInputTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                _isManualPaused = true;
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = false; //ECS Pause
                Time.timeScale = 0f; //Unity Pause
            }

            foreach((RefRO<LootTutorialResumeTag> _ , Entity entity) in SystemAPI.Query<RefRO<LootTutorialResumeTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = true; //ECS Resume
                Time.timeScale = 1f; //Unity Resume
            }

            foreach((RefRO<ResumeInputTag> _ , Entity entity) in SystemAPI.Query<RefRO<ResumeInputTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                int executeResume = math.select(1 , 0 , hasLootTag);
                _isManualPaused = false;
                
                for(int i = 0 ; i < executeResume ; i++)
                {
                    systemState.World.GetExistingSystemManaged<GameplaySystemGroup>().Enabled = true; //ECS Resume
                    Time.timeScale = 1f; //Unity Resume
                }
            }
        }
    }
}