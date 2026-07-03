namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct EvaluateLevelLoseConditionSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EnemySpawnerTag>(); }
        
        public void OnUpdate(ref SystemState systemState)
        {
            bool hasLost = false;

            foreach(var currentHealthComponent in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>()) { hasLost = currentHealthComponent.ValueRO.Value <= 0f; }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithAll<LevelLostTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                bool isAlreadyLost = SystemAPI.IsComponentEnabled<LevelLostTag>(entity);
                SystemAPI.SetComponentEnabled<LevelLostTag>(entity , isAlreadyLost || hasLost);
            }
        }
    }
}