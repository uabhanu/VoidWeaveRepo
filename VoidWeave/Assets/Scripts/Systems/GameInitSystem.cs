using Components;
using Unity.Burst;
using Unity.Entities;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GameInitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            systemState.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EnemySpawnerEntityComponent , InitializeGameTag , PlayerEntityComponent , TurretConfigEntityComponent>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            Entity entity = SystemAPI.GetSingletonEntity<InitializeGameTag>();
            EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            
            EnemySpawnerEntityComponent enemySpawnerEntityComponent = SystemAPI.GetComponent<EnemySpawnerEntityComponent>(entity);
            PlayerEntityComponent playerEntityComponent = SystemAPI.GetComponent<PlayerEntityComponent>(entity);
            TurretConfigEntityComponent turretConfigEntityComponent = SystemAPI.GetComponent<TurretConfigEntityComponent>(entity);
            
            entityCommandBuffer.Instantiate(enemySpawnerEntityComponent.Entity);
            entityCommandBuffer.Instantiate(playerEntityComponent.Entity);
            entityCommandBuffer.Instantiate(turretConfigEntityComponent.Entity);
            
            entityCommandBuffer.RemoveComponent<InitializeGameTag>(entity);
        }
    }
}