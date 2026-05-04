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
            GameEventsSystem.OnDamageTaken += OnDamageTaken;
            GameEventsSystem.OnDashPerformed += OnDashPerformed;
            GameEventsSystem.OnEnemyDeath += OnEnemyDeath;
            GameEventsSystem.OnProjectileFired += OnProjectileFired;
            GameEventsSystem.OnPlayerDeath += OnPlayerDeath;
            GameEventsSystem.OnTurretCooldownStarted += OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
        }

        private void OnDisable()
        {
            GameEventsSystem.OnDashPerformed -= OnDashPerformed;
            GameEventsSystem.OnEnemyDeath -= OnEnemyDeath;
            GameEventsSystem.OnProjectileFired -= OnProjectileFired;
            GameEventsSystem.OnPlayerDeath -= OnPlayerDeath;
            GameEventsSystem.OnTurretCooldownStarted -= OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
        }
        
        #endregion
        
        #region User Defined Event Listeners
        
        private void OnDamageTaken(float currentHealth)
        {
            Debug.Log("Play Damage Sound");
            
            if(damageTakenClip) sfxSource.PlayOneShot(damageTakenClip);
        }

        private void OnDashPerformed()
        {
            Debug.Log("Play Dash Sound");
            
            if(dashClip) sfxSource.PlayOneShot(dashClip);
        }
        
        private void OnEnemyDeath()
        {
            Debug.Log("Play Enemy Death Sound");
            
            if(enemyDeathClip) sfxSource.PlayOneShot(enemyDeathClip);
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log("Play Player Death Sound");
            
            if(playerDeathClip) sfxSource.PlayOneShot(playerDeathClip);
        }

        private void OnProjectileFired(float3 position)
        {
            Debug.Log("Play Projectile Fire Sound");
            
            if(projectileFiredClip) sfxSource.PlayOneShot(projectileFiredClip);
        }

        private void OnTurretCooldownStarted(Entity entity , float timer , float3 worldPosition)
        {
            Debug.Log("Play Turret Cooldown Sound");
            
            if(turretCooldownClip) sfxSource.PlayOneShot(turretCooldownClip);
        }

        private void OnWavePrepCountdownStarted(float timer , int waveState)
        {
            Debug.Log("Play Wave Prep Cooldown Sound");
            
            if(wavePrepClip && timer > 0) sfxSource.PlayOneShot(wavePrepClip);
        }
        
        #endregion
    }
}