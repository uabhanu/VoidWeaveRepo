namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class ManagedEventBridgeSystem : SystemBase
    {
        #region Variables

        private bool _previousGameFinishedState;
        private bool _previousLevelLostState;
        private bool _previousLevelWonState;
        private bool _previousLootTutorialState;
        private bool _previousTurretsTutorialState;

        private Entity _previousSelectedTurretEntity;

        private EntityQuery _beamTurretQuery;
        private EntityQuery _gameFinishedQuery;
        private EntityQuery _levelLostQuery;
        private EntityQuery _levelWonQuery;
        private EntityQuery _scatterTurretQuery;
        private EntityQuery _strikerTurretQuery;

        private EntityQuery _lootTutorialActiveQuery;
        private EntityQuery _turretsTutorialActiveQuery;

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
        public static event Action OnGameFinished;
        public static event Action OnHealthValueChanged;
        public static event Action OnLevelLost;
        public static event Action<int> OnLevelValueChanged;
        public static event Action OnLevelWon;
        public static event Action<bool> OnLootTutorialStateChanged;
        public static event Action<float , float3> OnPlayerDashCooldownStarted;
        public static event Action OnPlayerDeath;
        public static event Action<Entity , float , float3> OnTurretCooldownStarted;
        public static event Action<int , bool , int , string , int> OnTurretsTutorialStateChanged;
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

            _gameFinishedQuery = SystemAPI.QueryBuilder().WithAll<GameFinishedTag>().Build();
            _lootTutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<LootTutorialActiveTag>().Build();
            _turretsTutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag , TurretsTutorialActiveTag>().Build();

            _levelLostQuery = SystemAPI.QueryBuilder().WithAll<LevelLostTag, EnemySpawnerTag>().Build();
            _levelWonQuery = SystemAPI.QueryBuilder().WithAll<LevelWonTag, EnemySpawnerTag>().Build();
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

            bool currentGameFinishedState = !_gameFinishedQuery.IsEmpty;
            bool gameFinishedStateToggled = currentGameFinishedState != _previousGameFinishedState;
            _previousGameFinishedState = currentGameFinishedState;

            bool currentLevelLostState = !_levelLostQuery.IsEmpty;
            bool levelLostStateToggled = currentLevelLostState & !_previousLevelLostState;
            _previousLevelLostState = currentLevelLostState;

            bool currentLevelWonState = !_levelWonQuery.IsEmpty;
            bool levelWonStateToggled = currentLevelWonState & !_previousLevelWonState;
            _previousLevelWonState = currentLevelWonState;

            bool currentLootTutorialState = !_lootTutorialActiveQuery.IsEmpty;
            bool lootTutorialStateToggled = currentLootTutorialState != _previousLootTutorialState;
            _previousLootTutorialState = currentLootTutorialState;

            bool currentTurretsTutorialState = !_turretsTutorialActiveQuery.IsEmpty;
            bool turretsTutorialStateToggled = currentTurretsTutorialState != _previousTurretsTutorialState;
            _previousTurretsTutorialState = currentTurretsTutorialState;

            int currentLevel = SystemAPI.GetSingleton<LevelComponent>().Value;
            int selectedTurretCostComponent = SystemAPI.GetSingleton<SelectedTurretCostComponent>().Value;

            Entity currentSelectedTurretEntity = SystemAPI.GetSingleton<SelectedTurretEntityComponent>().Entity;
            bool selectionChanged = currentSelectedTurretEntity != _previousSelectedTurretEntity;
            int turretSelectionChanged = math.select(noAction , doAction , selectionChanged);
            _previousSelectedTurretEntity = currentSelectedTurretEntity;

            int gameFinishedTrigger = math.select(noAction , doAction , gameFinishedStateToggled & currentGameFinishedState);

            bool isStrikerTurret = currentSelectedTurretEntity == strikerTurretEntity;
            bool isScatterTurret = currentSelectedTurretEntity == scatterTurretEntity;
            bool isBeamTurret = currentSelectedTurretEntity == beamTurretEntity;

            int strikerTurretID = math.select(noAction , doAction , isStrikerTurret);
            int scatterTurretID = math.select(noAction , scatterTurretUnlockLevel , isScatterTurret);
            int beamTurretID = math.select(noAction , beamTurretUnlockLevel , isBeamTurret);

            int turretType = strikerTurretID + scatterTurretID + beamTurretID;
            string turretName = isStrikerTurret ? "Striker Turret" : (isScatterTurret ? "Scatter Turret" : (isBeamTurret ? "Beam Turret" : "Unknown"));

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

            for(int i = noAction ; i < gameFinishedTrigger ; i++) { OnGameFinished?.Invoke(); }

            foreach(var _ in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>().WithChangeFilter<CurrentHealthComponent>()) { OnHealthValueChanged?.Invoke(); }
            
            for(int i = noAction ; i < math.select(noAction , doAction , levelLostStateToggled) ; i++) { OnLevelLost?.Invoke(); }

            for(int i = noAction ; i < math.select(noAction , doAction , levelWonStateToggled) ; i++) { OnLevelWon?.Invoke(); }

            for(int i = noAction ; i < math.select(noAction , doAction , lootTutorialStateToggled) ; i++) { OnLootTutorialStateChanged?.Invoke(currentLootTutorialState); }

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

            // DEPLOYMENT SOUND (Runs ONLY for new turrets) ---
            foreach((RefRO<CooldownComponent> cooldownComponent , Entity turretEntity) in SystemAPI.Query<RefRO<CooldownComponent>>().WithAll<DeployingTurretTag>().WithEntityAccess())
            {
                int turretCooldownFinished = math.select(noAction , doAction , cooldownComponent.ValueRO.Value <= noAction);

                for(int i = noAction ; i < turretCooldownFinished ; i++)
                {
                    AudioManagerOnTurretCooldownFinished?.Invoke();
                    ecb.RemoveComponent<DeployingTurretTag>(turretEntity);
                }
            }

            // UI TIMERS (Runs for ALL turrets) ---
            // Because this query doesn't filter by the new tag, your UI still updates perfectly for both deployment and combat!
            foreach((RefRO<CooldownComponent> cooldownComponent , RefRO<LocalTransform> transform , Entity turretEntity) in SystemAPI.Query<RefRO<CooldownComponent> , RefRO<LocalTransform>>().WithAll<DeployingTurretTag>().WithAny<BeamTurretTag , ScatterTurretTag , StrikerTurretTag>().WithChangeFilter<CooldownComponent>().WithEntityAccess()) { OnTurretCooldownStarted?.Invoke(turretEntity , cooldownComponent.ValueRO.Value , transform.ValueRO.Position); }

            for(int i = noAction ; i < math.select(noAction , doAction , turretsTutorialStateToggled || (currentTurretsTutorialState && turretSelectionChanged == doAction)) ; i++) { OnTurretsTutorialStateChanged?.Invoke(currentLevel , currentTurretsTutorialState , selectedTurretCostComponent , turretName , turretType); }

            foreach((RefRO<TimerComponent> timerComponent , RefRO<WaveStateComponent> waveStateComponent) in SystemAPI.Query<RefRO<TimerComponent> , RefRO<WaveStateComponent>>().WithChangeFilter<TimerComponent>())
            {
                for(int t = noAction ; t < math.select(doAction , noAction , currentTurretsTutorialState) ; t++)
                {
                    OnWavePrepCountdownStarted?.Invoke(timerComponent.ValueRO.Value , waveStateComponent.ValueRO.Value);

                    int startLoop = SystemAPI.GetSingleton<WaveStateComponent>().Value;
                    int endLoop = math.select(startLoop , doAction , waveStateComponent.ValueRO.Value == startLoop && timerComponent.ValueRO.Value > SystemAPI.GetSingleton<InputNoneComponent>().Value && (int)timerComponent.ValueRO.Value != (int)(timerComponent.ValueRO.Value + SystemAPI.Time.DeltaTime));

                    for(int i = startLoop ; i < endLoop ; i++) { AudioManagerOnWavePrepCountdownStarted?.Invoke(); }
                }
            }
        }

        #endregion
    }
}