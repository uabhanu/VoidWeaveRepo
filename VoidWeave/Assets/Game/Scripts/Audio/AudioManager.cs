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
            GameEventsSystem.OnPlayerDeath += OnPlayerDeath;
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
            GameEventsSystem.OnPlayerDeath -= OnPlayerDeath;
        }
        
        #endregion
        
        #region User Defined Event Listeners
        
        private void OnDamageTakenByEnemy()
        {
            sfxSource.PlayOneShot(damageTakenByEnemyClip);
        }
        
        private void OnDamageTakenByPlayer()
        {
            sfxSource.PlayOneShot(damageTakenByPlayerClip);
        }

        private void OnDashPerformed()
        {
            sfxSource.PlayOneShot(dashClip);
        }
        
        private void OnEnemyDeath()
        {
            sfxSource.PlayOneShot(enemyDeathClip);
        }
        
        private void OnPlayerDeath()
        {
            sfxSource.PlayOneShot(playerDeathClip);
        }

        private void OnProjectileFiredByEnemy()
        {
            sfxSource.PlayOneShot(projectileFiredByEnemyClip);
        }
        
        private void OnProjectileFiredByBeamTurret()
        {
            sfxSource.PlayOneShot(projectileFiredByBeamTurretClip);
        }
        
        private void OnProjectileFiredByScatterTurret()
        {
            sfxSource.PlayOneShot(projectileFiredByScatterTurretClip);
        }
        
        private void OnProjectileFiredByStrikerTurret()
        {
            sfxSource.PlayOneShot(projectileFiredByStrikerTurretClip);
        }

        private void OnTurretCooldownFinished()
        {
            sfxSource.PlayOneShot(turretCooldownCompleteClip);
        }

        private void OnWavePrepCountdownStarted()
        {
            sfxSource.PlayOneShot(wavePrepClip);
        }
        
        #endregion
    }
}