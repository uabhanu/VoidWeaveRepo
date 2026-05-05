namespace Game.Scripts.Audio
{
    using Systems;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class AudioManager : MonoBehaviour
    {
        #region Variables
        
        [SerializeField] private AudioClip dashClip;
        [SerializeField] private AudioClip damageTakenClip;
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
            GameEventsSystem.AudioManagerOnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
            GameEventsSystem.OnDamageTaken += OnDamageTaken;
            GameEventsSystem.OnDashPerformed += OnDashPerformed;
            GameEventsSystem.OnEnemyDeath += OnEnemyDeath;
            GameEventsSystem.OnProjectileFired += OnProjectileFired;
            GameEventsSystem.OnPlayerDeath += OnPlayerDeath;
            GameEventsSystem.OnTurretCooldownStarted += OnTurretCooldownStarted;
        }

        private void OnDisable()
        {
            GameEventsSystem.AudioManagerOnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
            GameEventsSystem.OnDashPerformed -= OnDashPerformed;
            GameEventsSystem.OnEnemyDeath -= OnEnemyDeath;
            GameEventsSystem.OnProjectileFired -= OnProjectileFired;
            GameEventsSystem.OnPlayerDeath -= OnPlayerDeath;
            GameEventsSystem.OnTurretCooldownStarted -= OnTurretCooldownStarted;
        }
        
        #endregion
        
        #region User Defined Event Listeners
        
        private void OnDamageTaken(float currentHealth)
        {
            Debug.Log("Play Damage Sound");
            sfxSource.PlayOneShot(damageTakenClip);
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

        private void OnTurretCooldownStarted(Entity entity , float timer , float3 worldPosition)
        {
            Debug.Log("Play Turret Cooldown Sound");
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