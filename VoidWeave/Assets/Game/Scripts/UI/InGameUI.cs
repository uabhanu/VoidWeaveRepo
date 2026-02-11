namespace Game.Scripts.UI
{
    using Components;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        private EntityQuery _energyQuery;
        private EntityQuery _healthQuery;
        
        private EntityManager _entityManager;
        
        private Label _energyValueLabel;
        private Label _healthValueLabel;
        
        [SerializeField] private Color energyBarColor = Color.yellow;
        [SerializeField] private Color healthBarColor = Color.green;
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            if(!uiDocument) uiDocument = GetComponent<UIDocument>();
            
            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;
            
            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));

            var rootVisualElement = uiDocument.rootVisualElement;

            var energyTextLabel = rootVisualElement.Q<Label>("EnergyTextLabel");
            _energyValueLabel = rootVisualElement.Q<Label>("EnergyValueLabel");

            var healthTextLabel = rootVisualElement.Q<Label>("HealthTextLabel");
            _healthValueLabel = rootVisualElement.Q<Label>("HealthValueLabel");

            if(energyTextLabel != null) energyTextLabel.style.backgroundColor = energyBarColor;
            if(_energyValueLabel != null) _energyValueLabel.style.backgroundColor = energyBarColor;

            if(healthTextLabel != null) healthTextLabel.style.backgroundColor = healthBarColor;
            if(_healthValueLabel != null) _healthValueLabel.style.backgroundColor = healthBarColor;
        }
        
        private void Update()
        {
            if(_energyValueLabel != null && !_energyQuery.IsEmptyIgnoreFilter)
            {
                var energy = _energyQuery.GetSingleton<CurrentEnergyComponent>().Value;
                _energyValueLabel.text = $"{energy:F0}";
            }

            if(_healthValueLabel != null && !_healthQuery.IsEmptyIgnoreFilter)
            {
                var health = _healthQuery.GetSingleton<CurrentHealthComponent>().Value;
                _healthValueLabel.text = $"{health:F0}";
            }
        }
    }
}