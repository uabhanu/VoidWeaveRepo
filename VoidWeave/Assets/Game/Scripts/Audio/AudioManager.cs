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
        [SerializeField] private AudioClip projectileFiredByPlayerClip;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip turretCooldownClip;
        [SerializeField] private AudioClip wavePrepClip;
        
        #endregion
        
        #region Unity Callbacks

        private void OnEnable()
        {
            GameEventsSystem.AudioManagerOnDamageTakenByEnemy += OnDamageTakenByEnemy;
            GameEventsSystem.AudioManagerOnDamageTakenByPlayer += OnDamageTakenByPlayer;
            GameEventsSystem.AudioManagerOnProjectileFiredByEnemy += OnProjectileFiredByEnemy;
            GameEventsSystem.AudioManagerOnProjectileFiredByPlayer += OnProjectileFiredByPlayer;
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
            GameEventsSystem.AudioManagerOnProjectileFiredByPlayer -= OnProjectileFiredByPlayer;
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
        
        private void OnProjectileFiredByPlayer()
        {
            sfxSource.PlayOneShot(projectileFiredByPlayerClip);
        }

        private void OnTurretCooldownFinished()
        {
            sfxSource.PlayOneShot(turretCooldownClip);
        }

        private void OnWavePrepCountdownStarted()
        {
            sfxSource.PlayOneShot(wavePrepClip);
        }
        
        #endregion
    }
}