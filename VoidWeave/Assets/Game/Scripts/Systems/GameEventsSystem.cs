namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public partial class GameEventsSystem : SystemBase
    {
        public static Action OnPauseButtonClicked;

        public static event Action<float> OnDamageTaken;
        public static event Action OnDashPerformed;
        public static event Action OnEnemyDeath;
        public static event Action<float> OnEnergyValueChanged;
        public static event Action<float> OnHealthValueChanged;
        public static event Action<int> OnLevelValueChanged;
        public static event Action OnPlayerDeath;
        public static event Action<float3> OnProjectileFired;
        public static event Action<Entity , float , float3> OnTurretCooldownStarted;
        public static event Action<float , int> OnWavePrepCountdownStarted;

        protected override void OnUpdate()
        {
            foreach(RefRO<CurrentEnergyComponent> currentEnergyComponent in SystemAPI.Query<RefRO<CurrentEnergyComponent>>().WithChangeFilter<CurrentEnergyComponent>()) OnEnergyValueChanged?.Invoke(currentEnergyComponent.ValueRO.Value);

            foreach(RefRO<CurrentHealthComponent> currentHealthComponent in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>().WithChangeFilter<CurrentHealthComponent>()) OnHealthValueChanged?.Invoke(currentHealthComponent.ValueRO.Value);

            foreach((RefRO<CooldownComponent> cooldownComponent , RefRO<LocalTransform> transform , Entity turretEntity) in SystemAPI.Query<RefRO<CooldownComponent> , RefRO<LocalTransform>>().WithAny<BeamTurretTag , ScatterTurretTag , StrikerTurretTag>().WithChangeFilter<CooldownComponent>().WithEntityAccess()) OnTurretCooldownStarted?.Invoke(turretEntity , cooldownComponent.ValueRO.Value , transform.ValueRO.Position);

            foreach(var (currentHealthComponent , _ , entity) in SystemAPI.Query<RefRO<CurrentHealthComponent> , RefRO<DamageEventComponent>>().WithAll<PlayerTag>().WithEntityAccess())
            {
                OnDamageTaken?.Invoke(currentHealthComponent.ValueRO.Value);
                EntityManager.RemoveComponent<DamageEventComponent>(entity);
            }

            foreach(RefRO<PlayerInputComponent> playerInputComponent in SystemAPI.Query<RefRO<PlayerInputComponent>>().WithAll<PlayerTag>())
            {
                for(int i = (int)SystemAPI.GetSingleton<InputNoneComponent>().Value ; i < (int)math.select(SystemAPI.GetSingleton<InputNoneComponent>().Value , math.countbits(playerInputComponent.ValueRO.Value & SystemAPI.GetSingleton<InputDashComponent>().Value) , SystemAPI.GetSingleton<DashCooldownComponent>().Value <= SystemAPI.GetSingleton<InputNoneComponent>().Value) ; i++) { OnDashPerformed?.Invoke(); }
            }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<DeathTag>>().WithAll<EnemyTag>().WithEntityAccess())
            {
                OnEnemyDeath?.Invoke();
                EntityManager.RemoveComponent<DeathTag>(entity);
            }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<DeathTag>>().WithAll<PlayerTag>().WithEntityAccess())
            {
                OnPlayerDeath?.Invoke();
                EntityManager.RemoveComponent<DeathTag>(entity);
            }

            foreach(RefRO<LevelComponent> levelComponent in SystemAPI.Query<RefRO<LevelComponent>>().WithChangeFilter<LevelComponent>()) OnLevelValueChanged?.Invoke(levelComponent.ValueRO.Value);

            foreach(RefRO<LocalTransform> localTransformComponent in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<ProjectileTag>().WithChangeFilter<LocalTransform>()) { OnProjectileFired?.Invoke(localTransformComponent.ValueRO.Position); }

            foreach((RefRO<TimerComponent> timerComponent , RefRO<WaveStateComponent> waveStateComponent) in SystemAPI.Query<RefRO<TimerComponent> , RefRO<WaveStateComponent>>()) OnWavePrepCountdownStarted?.Invoke(timerComponent.ValueRO.Value , waveStateComponent.ValueRO.Value);
        }
    }
}