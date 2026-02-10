namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine.UIElements;

    public partial class InGameUISystem : SystemBase
    {
        private Label _energyLabel;
        private Label _healthLabel;

        protected override void OnCreate() { RequireForUpdate<UIReadyComponent>(); }

        public void SetReferences(Label energyLabel , Label healthLabel)
        {
            _energyLabel = energyLabel;
            _healthLabel = healthLabel;
        }

        protected override void OnUpdate()
        {
            foreach(var currentEnergyComponent in SystemAPI.Query<RefRO<CurrentEnergyComponent>>()) { _energyLabel.text = $"{currentEnergyComponent.ValueRO.Value:F0}"; }
            
            foreach(var currentHealth in SystemAPI.Query<RefRO<CurrentHealthComponent>>().WithAll<PlayerTag>()) { _healthLabel.text = $"{currentHealth.ValueRO.Value:F0}"; }
        }
    }
}