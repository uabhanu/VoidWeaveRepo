namespace Game.Scripts.Audio
{
    using Systems;
    using Unity.Mathematics;
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
        [SerializeField] private AudioClip projectileFiredClip;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip turretCooldownClip;
        [SerializeField] private AudioClip wavePrepClip;
        
        #endregion
        
        #region Unity Callbacks

        private void OnEnable()
        {
            GameEventsSystem.AudioManagerOnDamageTakenByEnemy += OnDamageTakenByEnemy;
            GameEventsSystem.AudioManagerOnDamageTakenByPlayer += OnDamageTakenByPlayer;
            GameEventsSystem.AudioManagerOnTurretCooldownFinished += OnTurretCooldownFinished;
            GameEventsSystem.AudioManagerOnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
            GameEventsSystem.OnDashPerformed += OnDashPerformed;
            GameEventsSystem.OnEnemyDeath += OnEnemyDeath;
            GameEventsSystem.OnProjectileFired += OnProjectileFired;
            GameEventsSystem.OnPlayerDeath += OnPlayerDeath;
        }

        private void OnDisable()
        {
            
            GameEventsSystem.AudioManagerOnDamageTakenByEnemy -= OnDamageTakenByEnemy;
            GameEventsSystem.AudioManagerOnDamageTakenByPlayer -= OnDamageTakenByPlayer;
            GameEventsSystem.AudioManagerOnTurretCooldownFinished -= OnTurretCooldownFinished;
            GameEventsSystem.AudioManagerOnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
            GameEventsSystem.OnDashPerformed -= OnDashPerformed;
            GameEventsSystem.OnEnemyDeath -= OnEnemyDeath;
            GameEventsSystem.OnProjectileFired -= OnProjectileFired;
            GameEventsSystem.OnPlayerDeath -= OnPlayerDeath;
        }
        
        #endregion
        
        #region User Defined Event Listeners
        
        private void OnDamageTakenByEnemy()
        {
            Debug.Log("Play Enemy Damage Sound");
            sfxSource.PlayOneShot(damageTakenByEnemyClip);
        }
        
        private void OnDamageTakenByPlayer()
        {
            Debug.Log("Play Player Damage Sound");
            sfxSource.PlayOneShot(damageTakenByPlayerClip);
        }

        private void OnDashPerformed()
        {
            Debug.Log("Play Dash Sound");
            sfxSource.PlayOneShot(dashClip);
        }
        
        private void OnEnemyDeath()
        {
            Debug.Log("Play Enemy Death Sound");
            sfxSource.PlayOneShot(enemyDeathClip);
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log("Play Player Death Sound");
            sfxSource.PlayOneShot(playerDeathClip);
        }

        private void OnProjectileFired(float3 position)
        {
            Debug.Log("Play Projectile Fire Sound");
            sfxSource.PlayOneShot(projectileFiredClip);
        }

        private void OnTurretCooldownFinished()
        {
            Debug.Log("Play Turret Cooldown Finished Sound");
            sfxSource.PlayOneShot(turretCooldownClip);
        }

        private void OnWavePrepCountdownStarted()
        {
            Debug.Log("Play Wave Prep Cooldown Sound");
            sfxSource.PlayOneShot(wavePrepClip);
        }
        
        #endregion
    }
}