namespace Game.Scripts.UI
{
    using Components;
    using System.Collections.Generic;
    using Systems;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        #region Variables

        private readonly List<Button> _inGameUIButtonsList = new();
        private readonly Dictionary<Entity , Label> _turretCooldownLabelsDictionary = new();

        private Button _continueButton;
        private Button _loseQuitButton;
        private Button _pauseButton;
        private Button _quitButton;
        private Button _restartButton;
        private Button _resumeButton;
        private Button _retryButton;

        private EntityQuery _boundaryYQuery;
        private EntityQuery _energyQuery;
        private EntityQuery _gameLostQuery;
        private EntityQuery _gameWonQuery;
        private EntityQuery _healthQuery;
        private EntityQuery _levelQuery;
        private EntityQuery _waveIndexQuery;

        private EntityManager _entityManager;

        private Label _energyValueLabel;
        private Label _healthValueLabel;
        private Label _levelValueLabel;
        private Label _wavePrepLabel;

        private VisualElement _loseScreenVisualElement;
        private VisualElement _pauseMenuVisualElement;
        private VisualElement _rootVisualElement;
        private VisualElement _winScreenVisualElement;

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
            if(world == null) return;
            _entityManager = world.EntityManager;

            _boundaryYQuery = _entityManager.CreateEntityQuery(typeof(ScreenBoundaryYComponent));

            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _gameLostQuery = _entityManager.CreateEntityQuery(typeof(GameLostTag));
            _gameWonQuery = _entityManager.CreateEntityQuery(typeof(GameWonTag));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));
            _waveIndexQuery = _entityManager.CreateEntityQuery(typeof(WaveIndexComponent));

            _entityManager.CreateEntity(typeof(ResumeInputTag));

            _rootVisualElement = uiDocument.rootVisualElement;

            _pauseButton = _rootVisualElement.Q<Button>("PauseButton");
            _pauseButton.clicked += () => { GameEventsSystem.OnPauseButtonClicked?.Invoke(); };

            _inGameUIButtonsList.Add(_pauseButton);
            _inGameUIButtonsList.AddRange(_rootVisualElement.Q("PauseMenuVisualElement").Query<Button>().ToList());

            _energyValueLabel = _rootVisualElement.Q<Label>("EnergyValueLabel");
            _healthValueLabel = _rootVisualElement.Q<Label>("HealthValueLabel");

            _levelValueLabel = _rootVisualElement.Q<Label>("LevelValueLabel");

            _pauseMenuVisualElement = _rootVisualElement.Q<VisualElement>("PauseMenuVisualElement");

            _quitButton = _pauseMenuVisualElement.Q<Button>("QuitButton");
            _quitButton.clicked += () => { GameEventsSystem.OnQuitButtonClicked?.Invoke(); };

            _restartButton = _pauseMenuVisualElement.Q<Button>("RestartButton");
            _restartButton.clicked += () => { GameEventsSystem.OnRestartButtonClicked?.Invoke(); };

            _resumeButton = _pauseMenuVisualElement.Q<Button>("ResumeButton");
            _resumeButton.clicked += () => { GameEventsSystem.OnResumeButtonClicked?.Invoke(); };

            _pauseMenuVisualElement.style.display = DisplayStyle.None;

            _loseScreenVisualElement = _rootVisualElement.Q<VisualElement>("LoseScreenVisualElement");

            if(_loseScreenVisualElement != null)
            {
                _inGameUIButtonsList.AddRange(_loseScreenVisualElement.Query<Button>().ToList());

                _loseQuitButton = _loseScreenVisualElement.Q<Button>("QuitButton");
                if(_loseQuitButton != null) _loseQuitButton.clicked += OnQuitButtonClicked;

                _retryButton = _loseScreenVisualElement.Q<Button>("RetryButton");
                if(_retryButton != null) _retryButton.clicked += OnRestartButtonClicked;

                _loseScreenVisualElement.style.display = DisplayStyle.None;
            }

            _winScreenVisualElement = _rootVisualElement.Q<VisualElement>("WinScreenVisualElement");

            if(_winScreenVisualElement != null)
            {
                _inGameUIButtonsList.AddRange(_winScreenVisualElement.Query<Button>().ToList());

                _continueButton = _winScreenVisualElement.Q<Button>("ContinueButton");

                if(_continueButton != null) _continueButton.clicked += OnContinueButtonClicked;

                _winScreenVisualElement.style.display = DisplayStyle.None;
            }
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
            var world = World.DefaultGameObjectInjectionWorld;

            if(world == null || !world.IsCreated) return;
            if(_entityManager.World != world) { RefreshEcsReferences(); }

            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + sineOffset) / sineDivisor;
            float alpha = Mathf.Lerp(minOpacity , maxOpacity , pulse);

            foreach(var button in _inGameUIButtonsList)
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

            if(!_gameWonQuery.IsEmpty && _winScreenVisualElement != null && _winScreenVisualElement.style.display == DisplayStyle.None)
            {
                _entityManager.CreateEntity(typeof(PauseInputTag));
                _pauseButton.SetEnabled(false);
                _winScreenVisualElement.style.display = DisplayStyle.Flex;
            }

            else if(!_gameLostQuery.IsEmpty && _loseScreenVisualElement != null && _loseScreenVisualElement.style.display == DisplayStyle.None)
            {
                _entityManager.CreateEntity(typeof(PauseInputTag));
                _pauseButton.SetEnabled(false);
                _loseScreenVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        #endregion

        #region Button Event Callbacks

        private void OnContinueButtonClicked()
        {
            Time.timeScale = 1f;

            if(_winScreenVisualElement != null) { _winScreenVisualElement.style.display = DisplayStyle.None; }

            _pauseButton.SetEnabled(true);

            if(!_gameWonQuery.IsEmpty)
            {
                Entity spawnerEntity = _entityManager.CreateEntityQuery(typeof(EnemySpawnerTag)).GetSingletonEntity();
                _entityManager.SetComponentEnabled<GameWonTag>(spawnerEntity , false);
            }

            _entityManager.CreateEntity(typeof(AdvanceLevelEventTag));
            _entityManager.CreateEntity(typeof(ResumeInputTag));
        }

        private void OnPauseButtonClicked()
        {
            _entityManager.CreateEntity(typeof(PauseInputTag));
            _pauseButton.SetEnabled(false);
            _pauseMenuVisualElement.style.display = DisplayStyle.Flex;
        }

        private void OnQuitButtonClicked()
        {
            Time.timeScale = 1f;

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnRestartButtonClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnResumeButtonClicked()
        {
            _entityManager.CreateEntity(typeof(ResumeInputTag));
            _pauseButton.SetEnabled(true);
            _pauseMenuVisualElement.style.display = DisplayStyle.None;
        }

        #endregion

        #region Event Callbacks

        private void OnEnergyValueChanged(float currentEnergy)
        {
            if(!_entityManager.World.IsCreated) return;
            _entityManager.CompleteDependencyBeforeRO<CurrentEnergyComponent>();
            if(!_energyQuery.IsEmptyIgnoreFilter) _energyValueLabel.text = $"{currentEnergy:F0}";
        }

        private void OnHealthValueChanged()
        {
            if(!_entityManager.World.IsCreated) return;

            _entityManager.CompleteDependencyBeforeRO<CurrentHealthComponent>();

            if(!_healthQuery.IsEmptyIgnoreFilter)
            {
                float playerHealth = _healthQuery.GetSingleton<CurrentHealthComponent>().Value;
                _healthValueLabel.text = $"{playerHealth:F0}";
            }
        }

        private void OnLevelValueChanged(int currentLevel)
        {
            if(!_entityManager.World.IsCreated) return;
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

            if(worldPosition.y >= boundaryY - turretCooldownTimerLabelFlipThreshold) { cooldownLabel.style.top = screenPoint.y + turretCooldownTimerLabelOffsetY; }
            else { cooldownLabel.style.top = screenPoint.y; }

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

            _entityManager.CompleteDependencyBeforeRO<WaveIndexComponent>();
            int waveIndex = _waveIndexQuery.IsEmptyIgnoreFilter ? 0 : _waveIndexQuery.GetSingleton<WaveIndexComponent>().Value;

            string wavePrefix = waveIndex switch
            {
                0 => "First Wave" ,
                1 => "Second Wave" ,
                _ => "Final Wave"
            };

            _wavePrepLabel.text = $"{wavePrefix} In\n{timer:F0}";
        }

        #endregion

        #region Custom Functions

        private void RefreshEcsReferences()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            if(world == null || !world.IsCreated) return;

            _entityManager = world.EntityManager;

            _boundaryYQuery = _entityManager.CreateEntityQuery(typeof(ScreenBoundaryYComponent));
            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _gameLostQuery = _entityManager.CreateEntityQuery(typeof(GameLostTag));
            _gameWonQuery = _entityManager.CreateEntityQuery(typeof(GameWonTag));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));
            _waveIndexQuery = _entityManager.CreateEntityQuery(typeof(WaveIndexComponent));

            _entityManager.CreateEntity(typeof(ResumeInputTag));
        }

        #endregion
    }
}