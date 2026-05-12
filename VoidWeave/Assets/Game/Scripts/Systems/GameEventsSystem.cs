namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public partial class GameEventsSystem : SystemBase
    {
        #region Variables

        public static Action OnPauseButtonClicked;

        public static event Action AudioManagerOnDamageTakenByEnemy;
        public static event Action AudioManagerOnDamageTakenByPlayer;
        public static event Action AudioManagerOnProjectileFiredByEnemy;
        public static event Action AudioManagerOnProjectileFiredByBeamTurret;
        public static event Action AudioManagerOnProjectileFiredByScatterTurret;
        public static event Action AudioManagerOnProjectileFiredByStrikerTurret;
        public static event Action AudioManagerOnTurretCooldownFinished;
        public static event Action AudioManagerOnWavePrepCountdownStarted;
        public static event Action OnDashPerformed;
        public static event Action OnEnemyDeath;
        public static event Action<float> OnEnergyValueChanged;
        public static event Action OnHealthValueChanged;
        public static event Action<int> OnLevelValueChanged;
        public static event Action OnPlayerDeath;
        public static event Action<Entity , float , float3> OnTurretCooldownStarted;
        public static event Action<float , int> OnWavePrepCountdownStarted;

        #endregion

        #region Unity Callbacks

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;

            foreach(var (_ , _ , entity) in SystemAPI.Query<RefRO<CurrentHealthComponent> , RefRO<DamageEventComponent>>().WithEntityAccess())
            {
                for(int i = noAction ; i < math.select(noAction , doAction , SystemAPI.HasComponent<EnemyTag>(entity)) ; i++) { AudioManagerOnDamageTakenByEnemy?.Invoke(); }

                for(int i = noAction ; i < math.select(noAction , doAction , SystemAPI.HasComponent<PlayerTag>(entity)) ; i++) { AudioManagerOnDamageTakenByPlayer?.Invoke(); }

                ecb.RemoveComponent<DamageEventComponent>(entity);
            }

            foreach(var (_ , entity) in SystemAPI.Query<DashPerformedTag>().WithEntityAccess())
            {
                OnDashPerformed?.Invoke();
                ecb.RemoveComponent<DashPerformedTag>(entity);
            }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<DeathTag>>().WithAll<EnemyTag>().WithEntityAccess())
            {
                OnEnemyDeath?.Invoke();
                ecb.RemoveComponent<DeathTag>(entity);
            }

            foreach(RefRO<CurrentEnergyComponent> currentEnergyComponent in SystemAPI.Query<RefRO<CurrentEnergyComponent>>().WithChangeFilter<CurrentEnergyComponent>()) OnEnergyValueChanged?.Invoke(currentEnergyComponent.ValueRO.Value);

            foreach(var _ in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>().WithChangeFilter<CurrentHealthComponent>()) { OnHealthValueChanged?.Invoke(); }

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<DeathTag>>().WithAll<PlayerTag>().WithEntityAccess())
            {
                OnPlayerDeath?.Invoke();
                ecb.RemoveComponent<DeathTag>(entity);
            }

            foreach(RefRO<LevelComponent> levelComponent in SystemAPI.Query<RefRO<LevelComponent>>().WithChangeFilter<LevelComponent>()) OnLevelValueChanged?.Invoke(levelComponent.ValueRO.Value);

            foreach(var (_ , entity) in SystemAPI.Query<RefRO<ProjectileFiredEventTag>>().WithEntityAccess())
            {
                for(int i = noAction ; i < math.select(noAction , doAction , SystemAPI.HasComponent<EnemyTag>(entity)) ; i++) { AudioManagerOnProjectileFiredByEnemy?.Invoke(); }

                for(int i = noAction ; i < math.select(noAction , doAction , SystemAPI.HasComponent<BeamTurretTag>(entity)) ; i++) { AudioManagerOnProjectileFiredByBeamTurret?.Invoke(); }

                for(int i = noAction ; i < math.select(noAction , doAction , SystemAPI.HasComponent<ScatterTurretTag>(entity)) ; i++) { AudioManagerOnProjectileFiredByScatterTurret?.Invoke(); }

                for(int i = noAction ; i < math.select(noAction , doAction , SystemAPI.HasComponent<StrikerTurretTag>(entity)) ; i++) { AudioManagerOnProjectileFiredByStrikerTurret?.Invoke(); }

                ecb.RemoveComponent<ProjectileFiredEventTag>(entity);
            }

            foreach((RefRO<CooldownComponent> cooldownComponent , RefRO<LocalTransform> transform , Entity turretEntity) in SystemAPI.Query<RefRO<CooldownComponent> , RefRO<LocalTransform>>().WithAny<BeamTurretTag , ScatterTurretTag , StrikerTurretTag>().WithChangeFilter<CooldownComponent>().WithEntityAccess())
            {
                OnTurretCooldownStarted?.Invoke(turretEntity , cooldownComponent.ValueRO.Value , transform.ValueRO.Position);

                int startLoop = (int)SystemAPI.GetSingleton<InputNoneComponent>().Value;
                int endLoop = math.select(startLoop , 1 , cooldownComponent.ValueRO.Value > SystemAPI.GetSingleton<InputNoneComponent>().Value && (int)cooldownComponent.ValueRO.Value != (int)(cooldownComponent.ValueRO.Value + SystemAPI.Time.DeltaTime));

                for(int i = startLoop ; i < endLoop ; i++) { AudioManagerOnTurretCooldownFinished?.Invoke(); }
            }

            foreach((RefRO<TimerComponent> timerComponent , RefRO<WaveStateComponent> waveStateComponent) in SystemAPI.Query<RefRO<TimerComponent> , RefRO<WaveStateComponent>>().WithChangeFilter<TimerComponent>())
            {
                OnWavePrepCountdownStarted?.Invoke(timerComponent.ValueRO.Value , waveStateComponent.ValueRO.Value);

                int startLoop = SystemAPI.GetSingleton<WaveStateComponent>().Value;
                int endLoop = math.select(startLoop , 1 , waveStateComponent.ValueRO.Value == startLoop && timerComponent.ValueRO.Value > SystemAPI.GetSingleton<InputNoneComponent>().Value && (int)timerComponent.ValueRO.Value != (int)(timerComponent.ValueRO.Value + SystemAPI.Time.DeltaTime));

                for(int i = startLoop ; i < endLoop ; i++) { AudioManagerOnWavePrepCountdownStarted?.Invoke(); }
            }
        }

        #endregion
    }
}