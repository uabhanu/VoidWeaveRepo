namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public partial class GameEventsSystem : SystemBase
    {
        public static event Action<float> OnEnergyValueChanged;
        public static event Action<float> OnHealthValueChanged;
        public static event Action<int> OnLevelValueChanged;
        public static Action OnPauseButtonClicked;
        public static Action OnRestartButtonClicked;
        public static Action OnResumeButtonClicked;
        public static event Action<Entity , float , float3> OnTurretCooldownStarted;
        public static event Action<float , int> OnWavePrepCountdownStarted;

        protected override void OnUpdate()
        {
            foreach(var currentEnergyComponent in SystemAPI.Query<RefRO<CurrentEnergyComponent>>().WithChangeFilter<CurrentEnergyComponent>()) { OnEnergyValueChanged?.Invoke(currentEnergyComponent.ValueRO.Value); }

            foreach(var currentHealthComponent in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>().WithChangeFilter<CurrentHealthComponent>()) { OnHealthValueChanged?.Invoke(currentHealthComponent.ValueRO.Value); }

            foreach(var levelComponent in SystemAPI.Query<RefRO<LevelComponent>>().WithChangeFilter<LevelComponent>()) { OnLevelValueChanged?.Invoke(levelComponent.ValueRO.Value); }

            foreach(var (cooldownComponent , transform , turretEntity) in SystemAPI.Query<RefRO<CooldownComponent> , RefRO<LocalTransform>>().WithAny<BeamTurretTag , ScatterTurretTag , StrikerTurretTag>().WithChangeFilter<CooldownComponent>().WithEntityAccess()) { OnTurretCooldownStarted?.Invoke(turretEntity , cooldownComponent.ValueRO.Value , transform.ValueRO.Position); }

            foreach(var (timerComponent , waveStateComponent) in SystemAPI.Query<RefRO<TimerComponent> , RefRO<WaveStateComponent>>()) { OnWavePrepCountdownStarted?.Invoke(timerComponent.ValueRO.Value , waveStateComponent.ValueRO.Value); }
        }
    }
}