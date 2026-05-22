namespace Game.Scripts.Audio
{
    using Systems;
    using UnityEngine;

    public class AudioManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private AudioClip dashClip;
        [SerializeField] private AudioClip damageTakenByEnemyClip;
        [SerializeField] private AudioClip damageTakenByPlayerClip;
        [SerializeField] private AudioClip enemyDeathClip;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip playerDeathClip;
        [SerializeField] private AudioClip projectileFiredByEnemyClip;
        [SerializeField] private AudioClip projectileFiredByBeamTurretClip;
        [SerializeField] private AudioClip projectileFiredByScatterTurretClip;
        [SerializeField] private AudioClip projectileFiredByStrikerTurretClip;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip turretCooldownCompleteClip;
        [SerializeField] private AudioClip wavePrepClip;

        #endregion

        #region Unity Callbacks

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

        private void OnDamageTakenByEnemy()
        {
            PlaySfx(damageTakenByEnemyClip);
        }

        private void OnDamageTakenByPlayer()
        {
            PlaySfx(damageTakenByPlayerClip);
        }

        private void OnDashPerformed()
        {
            PlaySfx(dashClip);
        }

        private void OnEnemyDeath()
        {
            PlaySfx(enemyDeathClip);
        }

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

        private void OnPlayerDeath()
        {
            PlaySfx(playerDeathClip);
        }

        private void OnProjectileFiredByEnemy()
        {
            PlaySfx(projectileFiredByEnemyClip);
        }

        private void OnProjectileFiredByBeamTurret()
        {
            PlaySfx(projectileFiredByBeamTurretClip);
        }

        private void OnProjectileFiredByScatterTurret()
        {
            PlaySfx(projectileFiredByScatterTurretClip);
        }

        private void OnProjectileFiredByStrikerTurret()
        {
            PlaySfx(projectileFiredByStrikerTurretClip);
        }

        private void OnTurretCooldownFinished()
        {
            PlaySfx(turretCooldownCompleteClip);
        }

        private void OnWavePrepCountdownStarted()
        {
            PlaySfx(wavePrepClip);
        }

        #endregion
        
        #region User Defined Custom Functions

        private void PlaySfx(AudioClip clip)
        {
            if(sfxSource && clip) { sfxSource.PlayOneShot(clip); }
        }
        
        #endregion
    }
}