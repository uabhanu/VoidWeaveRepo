namespace Game.Scripts.UI
{
    using Components;
    using Systems;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        #region Variables

        private Entity _trackingEntity = Entity.Null;

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
        private Label _timerLabel;

        private Camera _mainCamera;

        private VisualElement _rootVisualElement;

        [SerializeField] private Color energyLabelColor = Color.yellow;
        [SerializeField] private Color healthLabelColor = Color.green;
        [SerializeField] private Color levelLabelColor = Color.orange;
        [SerializeField] private Color timerLabelColor = Color.red;

        [SerializeField] private UIDocument uiDocument;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if(!uiDocument) uiDocument = GetComponent<UIDocument>();

            _mainCamera = Camera.main;

            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;

            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));

            _rootVisualElement = uiDocument.rootVisualElement;

            _energyTextLabel = _rootVisualElement.Q<Label>("EnergyTextLabel");
            _energyValueLabel = _rootVisualElement.Q<Label>("EnergyValueLabel");

            _healthTextLabel = _rootVisualElement.Q<Label>("HealthTextLabel");
            _healthValueLabel = _rootVisualElement.Q<Label>("HealthValueLabel");

            _levelTextLabel = _rootVisualElement.Q<Label>("LevelTextLabel");
            _levelValueLabel = _rootVisualElement.Q<Label>("LevelValueLabel");

            _timerLabel = _rootVisualElement.Q<Label>("TimerLabel");

            _energyTextLabel.style.backgroundColor = energyLabelColor;
            _energyValueLabel.style.backgroundColor = energyLabelColor;

            _healthTextLabel.style.backgroundColor = healthLabelColor;
            _healthValueLabel.style.backgroundColor = healthLabelColor;

            _levelTextLabel.style.backgroundColor = levelLabelColor;
            _levelValueLabel.style.backgroundColor = levelLabelColor;

            _timerLabel.style.backgroundColor = timerLabelColor;
        }

        private void OnEnable()
        {
            GameEventsSystem.OnEnergyValueChanged += OnEnergyValueChanged;
            GameEventsSystem.OnHealthValueChanged += OnHealthValueChanged;
            GameEventsSystem.OnLevelValueChanged += OnLevelValueChanged;
            GameEventsSystem.OnTurretCooldownStarted += OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
        }

        private void OnDisable()
        {
            GameEventsSystem.OnEnergyValueChanged -= OnEnergyValueChanged;
            GameEventsSystem.OnHealthValueChanged -= OnHealthValueChanged;
            GameEventsSystem.OnLevelValueChanged -= OnLevelValueChanged;
            GameEventsSystem.OnTurretCooldownStarted -= OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
        }

        #endregion

        #region Event Callbacks

        private void OnEnergyValueChanged(float currentEnergy)
        {
            _entityManager.CompleteDependencyBeforeRO<CurrentEnergyComponent>();

            if(!_energyQuery.IsEmptyIgnoreFilter) { _energyValueLabel.text = $"{currentEnergy:F0}"; }
        }

        private void OnHealthValueChanged(float currentHealth)
        {
            _entityManager.CompleteDependencyBeforeRO<CurrentHealthComponent>();

            if(!_healthQuery.IsEmptyIgnoreFilter) { _healthValueLabel.text = $"{currentHealth:F0}"; }
        }

        private void OnLevelValueChanged(int currentLevel)
        {
            _entityManager.CompleteDependencyBeforeRO<LevelComponent>();

            if(!_levelQuery.IsEmptyIgnoreFilter) { _levelValueLabel.text = $"{currentLevel:F0}"; }
        }

        private void OnTurretCooldownStarted(Entity entity , float timer , float3 worldPosition)
        {
            if(timer <= 0)
            {
                if(_trackingEntity == entity)
                {
                    _timerLabel.style.display = DisplayStyle.None;
                    _trackingEntity = Entity.Null;
                }

                return;
            }

            if(timer > 1.0f) { _trackingEntity = entity; }

            if(_trackingEntity != entity) return;

            _timerLabel.style.display = DisplayStyle.Flex;
            _timerLabel.text = $"{timer:F1}";

            Vector2 screenPos = RuntimePanelUtils.CameraTransformWorldToPanel(_rootVisualElement.panel , worldPosition , _mainCamera);

            _timerLabel.style.left = screenPos.x - 25;
            _timerLabel.style.top = screenPos.y - 50;
        }

        private void OnWavePrepCountdownStarted(float timer , int waveState)
        {
            if(waveState != 0 || timer <= 0)
            {
                _timerLabel.style.display = DisplayStyle.None;
                _timerLabel.text = string.Empty;
                return;
            }

            _timerLabel.style.display = DisplayStyle.Flex;
            _timerLabel.text = $"Next Wave In\n{timer:F0}";
            
            _timerLabel.style.translate = new Translate(Length.Percent(120) , Length.Percent(200) , 0);
        }

        #endregion
    }
}