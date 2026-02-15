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

        private Button _pauseButton;
        private Button _restartButton;
        private Button _resumeButton;

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
        private Label _wavePrepLabel;

        private VisualElement _rootVisualElement;
        private VisualElement _pauseMenuVisualElement;

        [SerializeField] private Color energyLabelColor;
        [SerializeField] private Color healthLabelColor;
        [SerializeField] private Color levelLabelColor;
        [SerializeField] private Color timerLabelBorderColor;
        [SerializeField] private Color timerLabelColor;

        [SerializeField] private float timerLabelAnchorPercent;
        [SerializeField] private float timerLabelBorderWidth;
        [SerializeField] private float timerLabelPadding;
        [SerializeField] private float timerLabelFontSize;
        [SerializeField] private float timerLabelHeight;
        [SerializeField] private float timerLabelOffsetX;
        [SerializeField] private float timerLabelOffsetY;
        [SerializeField] private float timerLabelTranslatePercentX;
        [SerializeField] private float timerLabelTranslatePercentY;
        [SerializeField] private float timerLabelTranslatePercentZ;
        [SerializeField] private float timerLabelWidth;
        [SerializeField] private float turretCooldownThreshold;
        [SerializeField] private float zeroThreshold;

        [SerializeField] private int waveStateReadyValue;

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

            _rootVisualElement = uiDocument.rootVisualElement;

            _pauseButton = _rootVisualElement.Q<Button>("PauseButton");
            _restartButton = _rootVisualElement.Q<Button>("RestartButton");
            _resumeButton = _rootVisualElement.Q<Button>("ResumeButton");

            _pauseButton.clicked += () => { GameEventsSystem.OnPauseButtonClicked?.Invoke(); };
            _restartButton.clicked += () => { GameEventsSystem.OnRestartButtonClicked?.Invoke(); };
            _resumeButton.clicked += () => { GameEventsSystem.OnResumeButtonClicked?.Invoke(); };

            _energyTextLabel = _rootVisualElement.Q<Label>("EnergyTextLabel");
            _energyValueLabel = _rootVisualElement.Q<Label>("EnergyValueLabel");

            _healthTextLabel = _rootVisualElement.Q<Label>("HealthTextLabel");
            _healthValueLabel = _rootVisualElement.Q<Label>("HealthValueLabel");

            _levelTextLabel = _rootVisualElement.Q<Label>("LevelTextLabel");
            _levelValueLabel = _rootVisualElement.Q<Label>("LevelValueLabel");

            _pauseMenuVisualElement = _rootVisualElement.Q<VisualElement>("PauseMenuVisualElement");
            _pauseMenuVisualElement.style.display = DisplayStyle.None;

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
            GameEventsSystem.OnPauseButtonClicked += OnPauseButtonClicked;
            GameEventsSystem.OnRestartButtonClicked += OnRestartButtonClicked;
            GameEventsSystem.OnResumeButtonClicked += OnResumeButtonClicked;
            GameEventsSystem.OnTurretCooldownStarted += OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
        }

        private void OnDisable()
        {
            GameEventsSystem.OnEnergyValueChanged -= OnEnergyValueChanged;
            GameEventsSystem.OnHealthValueChanged -= OnHealthValueChanged;
            GameEventsSystem.OnLevelValueChanged -= OnLevelValueChanged;
            GameEventsSystem.OnPauseButtonClicked -= OnPauseButtonClicked;
            GameEventsSystem.OnRestartButtonClicked -= OnRestartButtonClicked;
            GameEventsSystem.OnResumeButtonClicked -= OnResumeButtonClicked;
            GameEventsSystem.OnTurretCooldownStarted -= OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
        }

        #endregion

        #region Button Event Callbacks

        private void OnPauseButtonClicked()
        {
            _entityManager.CreateEntity(typeof(PauseInputTag));
            _pauseButton.SetEnabled(false);
            _pauseMenuVisualElement.style.display = DisplayStyle.Flex;
        }

        private void OnResumeButtonClicked()
        {
            _entityManager.CreateEntity(typeof(ResumeInputTag));
            _pauseButton.SetEnabled(true);
            _pauseMenuVisualElement.style.display = DisplayStyle.None;
        }

        private void OnRestartButtonClicked() { _entityManager.CreateEntity(typeof(RestartInputTag)); }

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

        private void OnTurretCooldownStarted(Entity entity , float timer , float3 worldPosition) {}

        private void OnWavePrepCountdownStarted(float timer , int waveState)
        {
            if(waveState != waveStateReadyValue || timer <= zeroThreshold)
            {
                if(_wavePrepLabel != null)
                {
                    _rootVisualElement.Remove(_wavePrepLabel);
                    _wavePrepLabel = null;
                }

                return;
            }

            if(_wavePrepLabel == null)
            {
                _wavePrepLabel = new Label
                {
                    style =
                    {
                        backgroundColor = timerLabelColor ,
                        borderBottomColor = timerLabelBorderColor ,
                        borderBottomWidth = timerLabelBorderWidth ,
                        borderLeftColor = timerLabelBorderColor ,
                        borderLeftWidth = timerLabelBorderWidth ,
                        borderRightColor = timerLabelBorderColor ,
                        borderRightWidth = timerLabelBorderWidth ,
                        borderTopColor = timerLabelBorderColor ,
                        borderTopWidth = timerLabelBorderWidth ,
                        fontSize = timerLabelFontSize ,
                        height = timerLabelHeight ,
                        left = Length.Percent(timerLabelAnchorPercent) ,
                        paddingBottom = timerLabelPadding ,
                        paddingLeft = timerLabelPadding ,
                        paddingRight = timerLabelPadding ,
                        paddingTop = timerLabelPadding ,
                        position = Position.Absolute ,
                        top = Length.Percent(timerLabelAnchorPercent) ,
                        translate = new Translate(Length.Percent(timerLabelTranslatePercentX) , Length.Percent(timerLabelTranslatePercentY) , timerLabelTranslatePercentZ) ,
                        unityFontStyleAndWeight = FontStyle.Bold ,
                        unityTextAlign = TextAnchor.MiddleCenter ,
                        width = timerLabelWidth
                    }
                };

                _rootVisualElement.Add(_wavePrepLabel);
            }

            _wavePrepLabel.text = $"Next Wave In\n{timer:F0}";
        }

        #endregion
    }
}