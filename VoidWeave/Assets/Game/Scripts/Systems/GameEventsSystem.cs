namespace Game.Scripts.Systems
{
    using Components;
    using System;
    using Unity.Entities;

    public partial class GameEventsSystem : SystemBase
    {
        public static event Action<float> OnEnergyValueChanged;
        public static event Action<float> OnHealthValueChanged;
        public static event Action<int> OnLevelValueChanged;

        protected override void OnUpdate()
        {
            foreach(var currentEnergy in SystemAPI.Query<RefRO<CurrentEnergyComponent>>().WithChangeFilter<CurrentEnergyComponent>())
            {
                OnEnergyValueChanged?.Invoke(currentEnergy.ValueRO.Value);
            }
            
            foreach(var currentHealth in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>().WithChangeFilter<CurrentHealthComponent>())
            {
                OnHealthValueChanged?.Invoke(currentHealth.ValueRO.Value);
            }
            
            foreach(var currentLevel in SystemAPI.Query<RefRO<LevelComponent>>().WithChangeFilter<LevelComponent>())
            {
                OnLevelValueChanged?.Invoke(currentLevel.ValueRO.Value);
            }
        }
    }
}