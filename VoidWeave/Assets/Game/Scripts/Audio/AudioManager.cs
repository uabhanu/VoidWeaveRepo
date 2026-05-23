namespace Game.Scripts.Audio
{
    using Components;
    using Systems;
    using Unity.Entities;
    using UnityEngine;

    public class AudioManager : MonoBehaviour
    {
        #region Variables

        private EntityManager _entityManager;
        private EntityQuery _muteQuery;

        [SerializeField] private AudioClip dashClip;
        [SerializeField] private AudioClip damageTakenByEnemyClip;
        [SerializeField] private AudioClip damageTakenByPlayerClip;
        [SerializeField] private AudioClip enemyDeathClip;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private int mutedVolume;
        [SerializeField] private AudioClip playerDeathClip;
        [SerializeField] private AudioClip projectileFiredByEnemyClip;
        [SerializeField] private AudioClip projectileFiredByBeamTurretClip;
        [SerializeField] private AudioClip projectileFiredByScatterTurretClip;
        [SerializeField] private AudioClip projectileFiredByStrikerTurretClip;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip turretCooldownCompleteClip;
        [SerializeField] private int unmutedVolume;
        [SerializeField] private AudioClip wavePrepClip;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if(world == null || !world.IsCreated) return;
            
            _entityManager = world.EntityManager;
            _muteQuery = _entityManager.CreateEntityQuery(typeof(MuteWhileTestingComponent));
            _entityManager.CompleteDependencyBeforeRO<MuteWhileTestingComponent>();

            float targetVolume = unmutedVolume;

            if(!_muteQuery.IsEmptyIgnoreFilter)
            {
                // Explicitly get all entities matching the query to avoid any ambiguity
                using var entities = _muteQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        
                // Always take the first one; this is inherently safe even if there are duplicates
                int isMuted = _entityManager.GetComponentData<MuteWhileTestingComponent>(entities[0]).Value;
        
                targetVolume = Mathf.Lerp(unmutedVolume , mutedVolume , isMuted);
            }

            AudioListener.volume = targetVolume;
        }

        private void OnEnable()
        {
            GameEventsSystem.AudioManagerOnDamageTakenByEnemy += OnDamageTakenByEnemy;
            GameEventsSystem.AudioManagerOnDamageTakenByPlayer += OnDamageTakenByPlayer;
            GameEventsSystem.AudioManagerOnProjectileFiredByEnemy += OnProjectileFiredByEnemy;
            GameEventsSystem.AudioManagerOnProjectileFiredByBeamTurret += OnProjectileFiredByBeamTurret;
            GameEventsSystem.AudioManagerOnProjectileFiredByScatterTurret += OnProjectileFiredByScatterTurret;
            GameEventsSystem.AudioManagerOnProjectileFiredByStrikerTurret += OnProjectileFiredByStrikerTurret;
            GameEventsSystem.AudioManagerOnTurretCooldownFinished += OnTurretCooldownFinished;
            GameEventsSystem.AudioManagerOnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
            GameEventsSystem.OnDashPerformed += OnDashPerformed;
            GameEventsSystem.OnEnemyDeath += OnEnemyDeath;
            GameEventsSystem.OnPauseButtonClicked += OnGamePaused;
            GameEventsSystem.OnPlayerDeath += OnPlayerDeath;
            GameEventsSystem.OnResumeButtonClicked += OnGameResumed;
        }

        private void OnDisable()
        {
            GameEventsSystem.AudioManagerOnDamageTakenByEnemy -= OnDamageTakenByEnemy;
            GameEventsSystem.AudioManagerOnDamageTakenByPlayer -= OnDamageTakenByPlayer;
            GameEventsSystem.AudioManagerOnProjectileFiredByEnemy -= OnProjectileFiredByEnemy;
            GameEventsSystem.AudioManagerOnProjectileFiredByBeamTurret -= OnProjectileFiredByBeamTurret;
            GameEventsSystem.AudioManagerOnProjectileFiredByScatterTurret -= OnProjectileFiredByScatterTurret;
            GameEventsSystem.AudioManagerOnProjectileFiredByStrikerTurret -= OnProjectileFiredByStrikerTurret;
            GameEventsSystem.AudioManagerOnTurretCooldownFinished -= OnTurretCooldownFinished;
            GameEventsSystem.AudioManagerOnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
            GameEventsSystem.OnDashPerformed -= OnDashPerformed;
            GameEventsSystem.OnEnemyDeath -= OnEnemyDeath;
            GameEventsSystem.OnPauseButtonClicked -= OnGamePaused;
            GameEventsSystem.OnPlayerDeath -= OnPlayerDeath;
            GameEventsSystem.OnResumeButtonClicked -= OnGameResumed;
        }

        #endregion

        #region User Defined Event Listeners

        private void OnDamageTakenByEnemy() { PlaySfx(damageTakenByEnemyClip); }

        private void OnDamageTakenByPlayer() { PlaySfx(damageTakenByPlayerClip); }

        private void OnDashPerformed() { PlaySfx(dashClip); }

        private void OnEnemyDeath() { PlaySfx(enemyDeathClip); }

        private void OnGamePaused()
        {
            if(musicSource) musicSource.Pause();
            if(sfxSource) sfxSource.Pause();
        }

        private void OnGameResumed()
        {
            if(musicSource) musicSource.UnPause();
            if(sfxSource) sfxSource.UnPause();
        }

        private void OnPlayerDeath() { PlaySfx(playerDeathClip); }

        private void OnProjectileFiredByEnemy() { PlaySfx(projectileFiredByEnemyClip); }

        private void OnProjectileFiredByBeamTurret() { PlaySfx(projectileFiredByBeamTurretClip); }

        private void OnProjectileFiredByScatterTurret() { PlaySfx(projectileFiredByScatterTurretClip); }

        private void OnProjectileFiredByStrikerTurret() { PlaySfx(projectileFiredByStrikerTurretClip); }

        private void OnTurretCooldownFinished() { PlaySfx(turretCooldownCompleteClip); }

        private void OnWavePrepCountdownStarted() { PlaySfx(wavePrepClip); }

        #endregion

        #region User Defined Custom Functions

        private void PlaySfx(AudioClip clip)
        {
            if(sfxSource && clip) { sfxSource.PlayOneShot(clip); }
        }

        private void RefreshEcsReferences()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if(world is not { IsCreated: true }) return;
            _entityManager = world.EntityManager;
            _muteQuery = _entityManager.CreateEntityQuery(typeof(MuteWhileTestingComponent));
        }

        #endregion
    }
}