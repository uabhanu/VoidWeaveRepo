namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct GameLoseSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EnemySpawnerTag>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            bool hasLost = false;

            foreach(var currentHealthComponent in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>()) { hasLost = currentHealthComponent.ValueRO.Value <= 0f; }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<EnemySpawnerTag>>().WithAll<GameLostTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                bool isAlreadyLost = SystemAPI.IsComponentEnabled<GameLostTag>(entity);
                SystemAPI.SetComponentEnabled<GameLostTag>(entity , isAlreadyLost || hasLost);
            }
        }
    }
}