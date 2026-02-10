namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine.UIElements;

    public partial class InGameUISystem : SystemBase
    {
        private VisualElement _healthBarFill;
        private Label _healthLabel;

        protected override void OnCreate()
        {
            var playerHealthQuery = SystemAPI.QueryBuilder().WithAll<CurrentHealthComponent , MaxHealthComponent , PlayerTag>().Build();
            
            RequireForUpdate(playerHealthQuery);
            RequireForUpdate<UIReadyComponent>();
        }

        public void SetReferences(VisualElement healthBarFillVisualElement , Label healthLabel)
        {
            _healthBarFill = healthBarFillVisualElement;
            _healthLabel = healthLabel;
        }

        protected override void OnUpdate()
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();

            var currentHealthComponent = SystemAPI.GetComponent<CurrentHealthComponent>(playerEntity).Value;
            var maxHealthComponent = SystemAPI.GetComponent<MaxHealthComponent>(playerEntity).Value;

            float percentage = currentHealthComponent / maxHealthComponent * 100f;

            _healthBarFill.style.width = Length.Percent(percentage);
            _healthLabel.text = $"{currentHealthComponent:F0}";
        }
    }
}