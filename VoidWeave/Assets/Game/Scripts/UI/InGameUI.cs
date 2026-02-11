namespace Game.Scripts.UI
{
    using Components;
    using Systems;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        #region Variables

        private EntityQuery _energyQuery;
        private EntityQuery _healthQuery;
        private EntityQuery _levelQuery;

        private EntityManager _entityManager;

        private Label _energyTextLabel;
        private Label _energyValueLabel;
        private Label _healthTextLabel;
        private Label _healthValueLabel;
        private Label _levelTextLabel;
        private Label _levelValueLabel;

        [SerializeField] private Color energyLabelColor = Color.yellow;
        [SerializeField] private Color healthLabelColor = Color.green;
        [SerializeField] private Color levelLabelColor = Color.orange;

        [SerializeField] private UIDocument uiDocument;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if(!uiDocument) uiDocument = GetComponent<UIDocument>();

            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;

            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));

            var rootVisualElement = uiDocument.rootVisualElement;

            _energyTextLabel = rootVisualElement.Q<Label>("EnergyTextLabel");
            _energyValueLabel = rootVisualElement.Q<Label>("EnergyValueLabel");

            _healthTextLabel = rootVisualElement.Q<Label>("HealthTextLabel");
            _healthValueLabel = rootVisualElement.Q<Label>("HealthValueLabel");

            _levelTextLabel = rootVisualElement.Q<Label>("LevelTextLabel");
            _levelValueLabel = rootVisualElement.Q<Label>("LevelValueLabel");

            _energyTextLabel.style.backgroundColor = energyLabelColor;
            _energyValueLabel.style.backgroundColor = energyLabelColor;

            _healthTextLabel.style.backgroundColor = healthLabelColor;
            _healthValueLabel.style.backgroundColor = healthLabelColor;

            _levelTextLabel.style.backgroundColor = levelLabelColor;
            _levelValueLabel.style.backgroundColor = levelLabelColor;
        }

        private void OnEnable()
        {
            GameEventsSystem.OnEnergyValueChanged += OnEnergyValueChanged;
            GameEventsSystem.OnHealthValueChanged += OnHealthValueChanged;
            GameEventsSystem.OnLevelValueChanged += OnLevelValueChanged;
        }
        
        private void OnDisable()
        {
            GameEventsSystem.OnEnergyValueChanged -= OnEnergyValueChanged;
            GameEventsSystem.OnHealthValueChanged -= OnHealthValueChanged;
            GameEventsSystem.OnLevelValueChanged -= OnLevelValueChanged;
        }
        
        #endregion
        
        #region Event Callbacks

        private void OnEnergyValueChanged(float currentEnergy)
        {
            _entityManager.CompleteDependencyBeforeRO<CurrentEnergyComponent>();
            
            if(!_energyQuery.IsEmptyIgnoreFilter)
            {
                _energyValueLabel.text = $"{currentEnergy:F0}";
            }
        }
        
        private void OnHealthValueChanged(float currentHealth)
        {
            _entityManager.CompleteDependencyBeforeRO<CurrentHealthComponent>();
            
            if(!_healthQuery.IsEmptyIgnoreFilter)
            {
                _healthValueLabel.text = $"{currentHealth:F0}";
            }
        }
        
        private void OnLevelValueChanged(int currentLevel)
        {
            _entityManager.CompleteDependencyBeforeRO<LevelComponent>();
            
            if(!_levelQuery.IsEmptyIgnoreFilter)
            {
                _levelValueLabel.text = $"{currentLevel:F0}";
            }
        }
        
        #endregion
    }
}