namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public partial class ManagedEventBridgeSystem : SystemBase
    {
        #region Variables

        private bool _previousTutorialState;
        private Entity _previousSelectedTurretEntity;
        private EntityQuery _tutorialActiveQuery;
        private EntityQuery _strikerTurretQuery;
        private EntityQuery _scatterTurretQuery;
        private EntityQuery _beamTurretQuery;

        public static Action OnPauseButtonClicked;
        public static Action OnQuitButtonClicked;
        public static Action OnRestartButtonClicked;
        public static Action OnResumeButtonClicked;

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
        public static event Action<float , float3> OnPlayerDashCooldownStarted;
        public static event Action OnPlayerDeath;
        public static event Action<Entity , float , float3> OnTurretCooldownStarted;
        public static event Action<int , bool , int , int> OnTutorialStateChanged;
        public static event Action<float , int> OnWavePrepCountdownStarted;

        #endregion

        #region Unity Callbacks

        protected override void OnCreate()
        {
            RequireForUpdate<BeamTurretUnlockLevelComponent>();
            RequireForUpdate<DoActionComponent>();
            RequireForUpdate<InputNoneComponent>();
            RequireForUpdate<LevelComponent>();
            RequireForUpdate<NoActionComponent>();
            RequireForUpdate<ScatterTurretUnlockLevelComponent>();
            RequireForUpdate<SelectedTurretCostComponent>();
            RequireForUpdate<SelectedTurretEntityComponent>();
            RequireForUpdate<WaveStateComponent>();

            _tutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag , TutorialActiveTag>().Build();

            _strikerTurretQuery = SystemAPI.QueryBuilder().WithAll<StrikerTurretTag , TurretEntityComponent>().Build();
            _scatterTurretQuery = SystemAPI.QueryBuilder().WithAll<ScatterTurretTag , TurretEntityComponent>().Build();
            _beamTurretQuery = SystemAPI.QueryBuilder().WithAll<BeamTurretTag , TurretEntityComponent>().Build();

            RequireForUpdate(_strikerTurretQuery);
            RequireForUpdate(_scatterTurretQuery);
            RequireForUpdate(_beamTurretQuery);
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            int beamTurretUnlockLevel = SystemAPI.GetSingleton<BeamTurretUnlockLevelComponent>().Value;
            int scatterTurretUnlockLevel = SystemAPI.GetSingleton<ScatterTurretUnlockLevelComponent>().Value;

            Entity strikerTurretEntity = _strikerTurretQuery.GetSingleton<TurretEntityComponent>().Entity;
            Entity scatterTurretEntity = _scatterTurretQuery.GetSingleton<TurretEntityComponent>().Entity;
            Entity beamTurretEntity = _beamTurretQuery.GetSingleton<TurretEntityComponent>().Entity;

            int doAction = SystemAPI.GetSingleton<DoActionComponent>().Value;
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;

            bool currentTutorialState = !_tutorialActiveQuery.IsEmpty;
            bool tutorialStateToggled = currentTutorialState != _previousTutorialState;
            _previousTutorialState = currentTutorialState;

            int currentLevel = SystemAPI.GetSingleton<LevelComponent>().Value;
            int selectedTurretCostComponent = SystemAPI.GetSingleton<SelectedTurretCostComponent>().Value;

            Entity currentSelectedTurretEntity = SystemAPI.GetSingleton<SelectedTurretEntityComponent>().Entity;
            bool selectionChanged = currentSelectedTurretEntity != _previousSelectedTurretEntity;
            int turretSelectionChanged = math.select(noAction , doAction , selectionChanged);
            _previousSelectedTurretEntity = currentSelectedTurretEntity;

            bool isStrikerTurret = currentSelectedTurretEntity == strikerTurretEntity;
            bool isScatterTurret = currentSelectedTurretEntity == scatterTurretEntity;
            bool isBeamTurret = currentSelectedTurretEntity == beamTurretEntity;

            int strikerTurretID = math.select(noAction , doAction , isStrikerTurret);
            int scatterTurretID = math.select(noAction , scatterTurretUnlockLevel , isScatterTurret);
            int beamTurretID = math.select(noAction , beamTurretUnlockLevel , isBeamTurret);

            int turretType = strikerTurretID + scatterTurretID + beamTurretID;

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

            foreach((RefRO<DashCooldownComponent> cooldownComponent , RefRO<LocalTransform> transform) in SystemAPI.Query<RefRO<DashCooldownComponent> , RefRO<LocalTransform>>().WithAll<PlayerTag>().WithChangeFilter<DashCooldownComponent>()) { OnPlayerDashCooldownStarted?.Invoke(cooldownComponent.ValueRO.Value , transform.ValueRO.Position); }

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

            for(int i = noAction ; i < math.select(noAction , doAction , tutorialStateToggled || (currentTutorialState && turretSelectionChanged == doAction)) ; i++) { OnTutorialStateChanged?.Invoke(currentLevel , currentTutorialState , selectedTurretCostComponent , turretType); }

            foreach((RefRO<TimerComponent> timerComponent , RefRO<WaveStateComponent> waveStateComponent) in SystemAPI.Query<RefRO<TimerComponent> , RefRO<WaveStateComponent>>().WithChangeFilter<TimerComponent>())
            {
                for(int t = noAction ; t < math.select(doAction , noAction , currentTutorialState) ; t++)
                {
                    OnWavePrepCountdownStarted?.Invoke(timerComponent.ValueRO.Value , waveStateComponent.ValueRO.Value);

                    int startLoop = SystemAPI.GetSingleton<WaveStateComponent>().Value;
                    int endLoop = math.select(startLoop , 1 , waveStateComponent.ValueRO.Value == startLoop && timerComponent.ValueRO.Value > SystemAPI.GetSingleton<InputNoneComponent>().Value && (int)timerComponent.ValueRO.Value != (int)(timerComponent.ValueRO.Value + SystemAPI.Time.DeltaTime));

                    for(int i = startLoop ; i < endLoop ; i++) { AudioManagerOnWavePrepCountdownStarted?.Invoke(); }
                }
            }
        }

        #endregion
    }
}