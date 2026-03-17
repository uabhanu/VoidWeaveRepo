namespace Game.Scripts.UI
{
    using Components;
    using System.Collections.Generic;
    using Systems;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        #region Variables

        private readonly Dictionary<Entity , Label> _turretCooldownLabelsDictionary = new();
        private EntityQuery _boundaryYQuery;
        private List<Button> _uiButtons = new();

        private Button _pauseButton;
        private Button _quitButton;
        private Button _restartButton;
        private Button _resumeButton;

        private EntityQuery _energyQuery;
        private EntityQuery _healthQuery;
        private EntityQuery _levelQuery;

        private EntityManager _entityManager;

        private Label _energyValueLabel;
        private Label _healthValueLabel;
        private Label _levelValueLabel;
        private Label _wavePrepLabel;

        private VisualElement _rootVisualElement;
        private VisualElement _pauseMenuVisualElement;

        [SerializeField] private Color turretCooldownTimerLabelBorderColour;
        [SerializeField] private Color turretCooldownTimerLabelBgColour;
        [SerializeField] private Color wavePrepTimerLabelSpriteTintColour;

        [SerializeField] private float maxOpacity;
        [SerializeField] private float minOpacity;
        [SerializeField] private float pulseSpeed;
        [SerializeField] private float sineDivisor;
        [SerializeField] private float sineOffset;
        [SerializeField] private float turretCooldownThreshold;
        [SerializeField] private float turretCooldownTimerLabelAnchorPercent;
        [SerializeField] private float turretCooldownTimerLabelBorderWidth;
        [SerializeField] private float turretCooldownTimerLabelFlipThreshold;
        [SerializeField] private float turretCooldownTimerLabelFontSize;
        [SerializeField] private float turretCooldownTimerLabelHeight;
        [SerializeField] private float turretCooldownTimerLabelOffsetX;
        [SerializeField] private float turretCooldownTimerLabelOffsetY;
        [SerializeField] private float turretCooldownTimerLabelPadding;
        [SerializeField] private float turretCooldownTimerLabelTranslatePercentX;
        [SerializeField] private float turretCooldownTimerLabelTranslatePercentY;
        [SerializeField] private float turretCooldownTimerLabelTranslatePercentZ;
        [SerializeField] private float turretCooldownTimerLabelWidth;
        [SerializeField] private float wavePrepTimerLabelAnchorPercent;
        [SerializeField] private float wavePrepTimerLabelBorderWidth;
        [SerializeField] private float wavePrepTimerLabelFontSize;
        [SerializeField] private float wavePrepTimerLabelHeight;
        [SerializeField] private float wavePrepTimerLabelOffsetX;
        [SerializeField] private float wavePrepTimerLabelOffsetY;
        [SerializeField] private float wavePrepTimerLabelTranslatePercentX;
        [SerializeField] private float wavePrepTimerLabelTranslatePercentY;
        [SerializeField] private float wavePrepTimerLabelTranslatePercentZ;
        [SerializeField] private float wavePrepTimerLabelWidth;
        [SerializeField] private float zeroOpacity;
        [SerializeField] private float zeroThreshold;

        [SerializeField] private int wavePrepLabelPaddingLeft;
        [SerializeField] private int waveStateReadyValue;

        [SerializeField] private Sprite hudPanelSprite;

        [SerializeField] private UIDocument uiDocument;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if(!uiDocument) uiDocument = GetComponent<UIDocument>();

            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;

            _boundaryYQuery = _entityManager.CreateEntityQuery(typeof(ScreenBoundaryYComponent));

            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));

            _rootVisualElement = uiDocument.rootVisualElement;

            _pauseButton = _rootVisualElement.Q<Button>("PauseButton");
            _quitButton = _rootVisualElement.Q<Button>("QuitButton");
            _restartButton = _rootVisualElement.Q<Button>("RestartButton");
            _resumeButton = _rootVisualElement.Q<Button>("ResumeButton");

            _uiButtons = _rootVisualElement.Query<Button>(null , "unity-button").ToList();

            _pauseButton.clicked += () => { GameEventsSystem.OnPauseButtonClicked?.Invoke(); };
            _quitButton.clicked += () => { GameEventsSystem.OnQuitButtonClicked?.Invoke(); };
            _restartButton.clicked += () => { GameEventsSystem.OnRestartButtonClicked?.Invoke(); };
            _resumeButton.clicked += () => { GameEventsSystem.OnResumeButtonClicked?.Invoke(); };

            _energyValueLabel = _rootVisualElement.Q<Label>("EnergyValueLabel");
            _healthValueLabel = _rootVisualElement.Q<Label>("HealthValueLabel");

            _levelValueLabel = _rootVisualElement.Q<Label>("LevelValueLabel");

            _pauseMenuVisualElement = _rootVisualElement.Q<VisualElement>("PauseMenuVisualElement");
            _pauseMenuVisualElement.style.display = DisplayStyle.None;
        }

        private void OnEnable()
        {
            GameEventsSystem.OnEnergyValueChanged += OnEnergyValueChanged;
            GameEventsSystem.OnHealthValueChanged += OnHealthValueChanged;
            GameEventsSystem.OnLevelValueChanged += OnLevelValueChanged;
            GameEventsSystem.OnPauseButtonClicked += OnPauseButtonClicked;
            GameEventsSystem.OnQuitButtonClicked += OnQuitButtonClicked;
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
            GameEventsSystem.OnQuitButtonClicked -= OnQuitButtonClicked;
            GameEventsSystem.OnRestartButtonClicked -= OnRestartButtonClicked;
            GameEventsSystem.OnResumeButtonClicked -= OnResumeButtonClicked;
            GameEventsSystem.OnTurretCooldownStarted -= OnTurretCooldownStarted;
            GameEventsSystem.OnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
        }

        private void Update()
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + sineOffset) / sineDivisor;
            float alpha = Mathf.Lerp(minOpacity , maxOpacity , pulse);

            foreach(var button in _uiButtons)
            {
                if(button != null)
                {
                    if(!button.enabledSelf)
                    {
                        button.style.opacity = zeroOpacity;
                        continue;
                    }

                    button.style.opacity = alpha;
                }
            }
        }

        #endregion

        #region Button Event Callbacks

        private void OnPauseButtonClicked()
        {
            _entityManager.CreateEntity(typeof(PauseInputTag));
            _pauseButton.SetEnabled(false);
            _pauseMenuVisualElement.style.display = DisplayStyle.Flex;
        }

        private void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #else
				Application.Quit();
            #endif
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

            if(!_energyQuery.IsEmptyIgnoreFilter) _energyValueLabel.text = $"{currentEnergy:F0}";
        }

        private void OnHealthValueChanged(float currentHealth)
        {
            _entityManager.CompleteDependencyBeforeRO<CurrentHealthComponent>();

            if(!_healthQuery.IsEmptyIgnoreFilter) _healthValueLabel.text = $"{currentHealth:F0}";
        }

        private void OnLevelValueChanged(int currentLevel)
        {
            _entityManager.CompleteDependencyBeforeRO<LevelComponent>();

            if(!_levelQuery.IsEmptyIgnoreFilter) _levelValueLabel.text = $"{currentLevel:F0}";
        }

        private void OnTurretCooldownStarted(Entity entity , float timer , float3 worldPosition)
        {
            if(timer <= turretCooldownThreshold)
            {
                if(_turretCooldownLabelsDictionary.TryGetValue(entity , out Label label))
                {
                    _rootVisualElement.Remove(label);
                    _turretCooldownLabelsDictionary.Remove(entity);
                }

                return;
            }

            if(!_turretCooldownLabelsDictionary.TryGetValue(entity , out Label cooldownLabel))
            {
                cooldownLabel = new Label
                {
                    style =
                    {
                        backgroundColor = turretCooldownTimerLabelBgColour ,
                        borderBottomColor = turretCooldownTimerLabelBorderColour ,
                        borderBottomWidth = turretCooldownTimerLabelBorderWidth ,
                        borderLeftColor = turretCooldownTimerLabelBorderColour ,
                        borderLeftWidth = turretCooldownTimerLabelBorderWidth ,
                        borderRightColor = turretCooldownTimerLabelBorderColour ,
                        borderRightWidth = turretCooldownTimerLabelBorderWidth ,
                        borderTopColor = turretCooldownTimerLabelBorderColour ,
                        borderTopWidth = turretCooldownTimerLabelBorderWidth ,
                        fontSize = turretCooldownTimerLabelFontSize ,
                        height = turretCooldownTimerLabelHeight ,
                        left = Length.Percent(turretCooldownTimerLabelAnchorPercent) ,
                        paddingBottom = turretCooldownTimerLabelPadding ,
                        paddingLeft = turretCooldownTimerLabelPadding ,
                        paddingRight = turretCooldownTimerLabelPadding ,
                        paddingTop = turretCooldownTimerLabelPadding ,
                        position = Position.Absolute ,
                        top = Length.Percent(turretCooldownTimerLabelAnchorPercent) ,
                        translate = new Translate(Length.Percent(turretCooldownTimerLabelTranslatePercentX) , Length.Percent(turretCooldownTimerLabelTranslatePercentY) , turretCooldownTimerLabelTranslatePercentZ) ,
                        unityFontStyleAndWeight = FontStyle.Bold ,
                        unityTextAlign = TextAnchor.MiddleCenter ,
                        width = turretCooldownTimerLabelWidth
                    }
                };

                _rootVisualElement.Add(cooldownLabel);
                _turretCooldownLabelsDictionary.Add(entity , cooldownLabel);
            }

            cooldownLabel.text = $"{timer:F0}";

            Vector2 screenPoint = RuntimePanelUtils.CameraTransformWorldToPanel(_rootVisualElement.panel , worldPosition , Camera.main);

            _entityManager.CompleteDependencyBeforeRO<ScreenBoundaryYComponent>();
            float boundaryY = _boundaryYQuery.GetSingleton<ScreenBoundaryYComponent>().Value;

            if(worldPosition.y >= boundaryY - turretCooldownTimerLabelFlipThreshold)
            {
                cooldownLabel.style.top = screenPoint.y + turretCooldownTimerLabelOffsetY;
            }
            else
            {
                cooldownLabel.style.top = screenPoint.y;
            }

            cooldownLabel.style.left = screenPoint.x;
        }

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
                        backgroundImage = new StyleBackground(hudPanelSprite) ,
                        unityBackgroundImageTintColor = wavePrepTimerLabelSpriteTintColour ,
                        backgroundColor = Color.clear ,
                        fontSize = wavePrepTimerLabelFontSize ,
                        height = wavePrepTimerLabelHeight ,
                        width = wavePrepTimerLabelWidth ,
                        position = Position.Absolute ,
                        paddingLeft = wavePrepLabelPaddingLeft ,
                        left = Length.Percent(wavePrepTimerLabelAnchorPercent) ,
                        top = Length.Percent(wavePrepTimerLabelAnchorPercent) ,
                        translate = new Translate(Length.Percent(wavePrepTimerLabelTranslatePercentX) , Length.Percent(wavePrepTimerLabelTranslatePercentY) , wavePrepTimerLabelTranslatePercentZ) ,
                        unityFontStyleAndWeight = FontStyle.Bold ,
                        unityTextAlign = TextAnchor.MiddleCenter ,
                        color = Color.white
                    }
                };

                _rootVisualElement.Add(_wavePrepLabel);

                _wavePrepLabel.SendToBack();
            }

            _wavePrepLabel.text = $"Next Wave In\n{timer:F0}";
        }

        #endregion
    }
}