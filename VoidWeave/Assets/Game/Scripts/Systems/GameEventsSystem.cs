namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public partial class GameEventsSystem : SystemBase
    {
        public static Action OnPauseButtonClicked;
        public static Action OnQuitButtonClicked;
        public static Action OnRestartButtonClicked;
        public static Action OnResumeButtonClicked;
        public static Action OnStartButtonClicked;
        public static event Action<float> OnEnergyValueChanged;
        public static event Action<float> OnHealthValueChanged;
        public static event Action<int> OnLevelValueChanged;
        public static event Action<Entity , float , float3> OnTurretCooldownStarted;
        public static event Action<float , int> OnWavePrepCountdownStarted;

        protected override void OnUpdate()
        {
            foreach(RefRO<CurrentEnergyComponent> currentEnergyComponent in SystemAPI.Query<RefRO<CurrentEnergyComponent>>().WithChangeFilter<CurrentEnergyComponent>()) OnEnergyValueChanged?.Invoke(currentEnergyComponent.ValueRO.Value);

            foreach(RefRO<CurrentHealthComponent> currentHealthComponent in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>().WithChangeFilter<CurrentHealthComponent>()) OnHealthValueChanged?.Invoke(currentHealthComponent.ValueRO.Value);

            foreach(RefRO<LevelComponent> levelComponent in SystemAPI.Query<RefRO<LevelComponent>>().WithChangeFilter<LevelComponent>()) OnLevelValueChanged?.Invoke(levelComponent.ValueRO.Value);

            foreach((RefRO<CooldownComponent> cooldownComponent , RefRO<LocalTransform> transform , Entity turretEntity) in SystemAPI.Query<RefRO<CooldownComponent> , RefRO<LocalTransform>>().WithAny<BeamTurretTag , ScatterTurretTag , StrikerTurretTag>().WithChangeFilter<CooldownComponent>().WithEntityAccess()) OnTurretCooldownStarted?.Invoke(turretEntity , cooldownComponent.ValueRO.Value , transform.ValueRO.Position);

            foreach((RefRO<TimerComponent> timerComponent , RefRO<WaveStateComponent> waveStateComponent) in SystemAPI.Query<RefRO<TimerComponent> , RefRO<WaveStateComponent>>()) OnWavePrepCountdownStarted?.Invoke(timerComponent.ValueRO.Value , waveStateComponent.ValueRO.Value);
        }
    }
}