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

        private const int Wave1Index = 0;
        private const int Wave2Index = 1;

        private readonly List<Button> _inGameUIButtonsList = new();

        private readonly Dictionary<Entity , Label> _playerDashCooldownLabelsDictionary = new();
        private readonly Dictionary<Entity , Label> _turretCooldownLabelsDictionary = new();

        private Button _continueButton;
        private Button _gameFinishedButton;
        private Button _loseQuitButton;
        private Button _pauseButton;
        private Button _quitButton;
        private Button _restartButton;
        private Button _resumeButton;
        private Button _retryButton;

        private EntityQuery _boundaryYQuery;
        private EntityQuery _energyQuery;
        private EntityQuery _healthQuery;
        private EntityQuery _lastLevelQuery;
        private EntityQuery _levelLostQuery;
        private EntityQuery _levelQuery;
        private EntityQuery _levelWonQuery;
        private EntityQuery _playerQuery;
        private EntityQuery _waveIndexQuery;

        private EntityManager _entityManager;

        private Label _energyValueLabel;
        private Label _healthValueLabel;
        private Label _levelValueLabel;
        private Label _turretsTutorialLabel;
        private Label _wavePrepLabel;

        private VisualElement _currentBlinkingVisualElement;
        private VisualElement _gameFinishedScreenVisualElement;
        private VisualElement _levelLoseScreenVisualElement;
        private VisualElement _levelWinScreenVisualElement;
        private VisualElement _lootTutorialLabel;
        private VisualElement _pauseMenuVisualElement;
        private VisualElement _rootVisualElement;

        [SerializeField] private Color playerDashCooldownTimerLabelBgColour;
        [SerializeField] private Color playerDashCooldownTimerLabelBorderColour;
        [SerializeField] private Color playerDashCooldownTimerLabelTextColour;
        [SerializeField] private Color beamTurretCooldownTimerLabelBgColour;
        [SerializeField] private Color beamTurretCooldownTimerLabelBorderColour;
        [SerializeField] private Color scatterTurretCooldownTimerLabelBgColour;
        [SerializeField] private Color scatterTurretCooldownTimerLabelBorderColour;
        [SerializeField] private Color strikerTurretCooldownTimerLabelBgColour;
        [SerializeField] private Color strikerTurretCooldownTimerLabelBorderColour;
        [SerializeField] private Color wavePrepTimerLabelSpriteTintColour;

        [SerializeField] private float characterCooldownTimerLabelAnchorPercent;
        [SerializeField] private float characterCooldownTimerLabelBorderWidth;
        [SerializeField] private float characterCooldownTimerLabelFlipThreshold;
        [SerializeField] private float characterCooldownTimerLabelFontSize;
        [SerializeField] private float characterCooldownTimerLabelHeight;
        [SerializeField] private float characterCooldownTimerLabelOffsetX;
        [SerializeField] private float characterCooldownTimerLabelOffsetY;
        [SerializeField] private float characterCooldownTimerLabelPadding;
        [SerializeField] private float characterCooldownTimerLabelTranslatePercentX;
        [SerializeField] private float characterCooldownTimerLabelTranslatePercentY;
        [SerializeField] private float characterCooldownTimerLabelTranslatePercentZ;
        [SerializeField] private float characterCooldownTimerLabelWidth;
        [SerializeField] private float lootTutorialLabelPaddingBottom;
        [SerializeField] private float lootTutorialLabelPaddingLeft;
        [SerializeField] private float lootTutorialLabelPaddingRight;
        [SerializeField] private float lootTutorialLabelPaddingTop;
        [SerializeField] private float lootTutorialLabelIconSize;
        [SerializeField] private float lootTutorialLabelIconMarginRight;
        [SerializeField] private float maxOpacity;
        [SerializeField] private float minOpacity;
        [SerializeField] private float pulseSpeed;
        [SerializeField] private float sineDivisor;
        [SerializeField] private float sineOffset;
        [SerializeField] private float tutorialLabelFontSize;
        [SerializeField] private float tutorialLabelBottomPercent;
        [SerializeField] private float tutorialLabelPaddingBottom;
        [SerializeField] private float tutorialLabelPaddingLeft;
        [SerializeField] private float tutorialLabelPaddingRight;
        [SerializeField] private float tutorialLabelPaddingTop;
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

        [SerializeField] private int turretIdNone;
        [SerializeField] private int turretIdStriker;
        [SerializeField] private int turretIdScatter;
        [SerializeField] private int turretIdBeam;
        [SerializeField] private int tutorialLevel1;
        [SerializeField] private int tutorialLevel2;
        [SerializeField] private int tutorialLevel3;
        [SerializeField] private int tutorialLevel4;
        [SerializeField] private int wave0Index;
        [SerializeField] private int wavePrepLabelPaddingLeft;
        [SerializeField] private int waveStateReadyValue;

        [SerializeField] private Sprite hudPanelSprite;

        [SerializeField] [Multiline] private string level1TutorialText;
        [SerializeField] [Multiline] private string level2TutorialText;
        [SerializeField] [Multiline] private string level3TutorialText;
        [SerializeField] [Multiline] private string level4TutorialText;
        [SerializeField] [Multiline] private string lootTutorialTextP1;
        [SerializeField] [Multiline] private string lootTutorialTextP2;
        [SerializeField] [Multiline] private string turretsTutorialText;

        [SerializeField] private string beamTurretName;
        [SerializeField] private string scatterTurretName;
        [SerializeField] private string strikerTurretName;
        [SerializeField] private string wave1Text;
        [SerializeField] private string wave2Text;
        [SerializeField] private string wave3Text;

        [SerializeField] private Texture2D lootTutorialTexture2D;

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
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _lastLevelQuery = _entityManager.CreateEntityQuery(typeof(LastLevelComponent));
            _levelLostQuery = _entityManager.CreateEntityQuery(typeof(LevelLostTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));
            _levelWonQuery = _entityManager.CreateEntityQuery(typeof(LevelWonTag));
            _playerQuery = _entityManager.CreateEntityQuery(typeof(PlayerTag));
            _waveIndexQuery = _entityManager.CreateEntityQuery(typeof(WaveIndexComponent));

            _entityManager.CreateEntity(typeof(ResumeInputTag));

            _rootVisualElement = uiDocument.rootVisualElement;

            _pauseButton = _rootVisualElement.Q<Button>("PauseButton");
            _pauseButton.clicked += () => { ManagedEventBridgeSystem.OnPauseButtonClicked?.Invoke(); };

            _inGameUIButtonsList.Add(_pauseButton);
            _inGameUIButtonsList.AddRange(_rootVisualElement.Q("PauseMenuVisualElement").Query<Button>().ToList());

            _energyValueLabel = _rootVisualElement.Q<Label>("EnergyValueLabel");
            _healthValueLabel = _rootVisualElement.Q<Label>("HealthValueLabel");

            _levelValueLabel = _rootVisualElement.Q<Label>("LevelValueLabel");

            _pauseMenuVisualElement = _rootVisualElement.Q<VisualElement>("PauseMenuVisualElement");

            _quitButton = _pauseMenuVisualElement.Q<Button>("QuitButton");
            _quitButton.clicked += () => { ManagedEventBridgeSystem.OnQuitButtonClicked?.Invoke(); };

            _restartButton = _pauseMenuVisualElement.Q<Button>("RestartButton");
            _restartButton.clicked += () => { ManagedEventBridgeSystem.OnRestartButtonClicked?.Invoke(); };

            _resumeButton = _pauseMenuVisualElement.Q<Button>("ResumeButton");
            _resumeButton.clicked += () => { ManagedEventBridgeSystem.OnResumeButtonClicked?.Invoke(); };

            _pauseMenuVisualElement.style.display = DisplayStyle.None;

            _gameFinishedScreenVisualElement = _rootVisualElement.Q<VisualElement>("GameFinishedScreenVisualElement");

            if(_gameFinishedScreenVisualElement != null)
            {
                _inGameUIButtonsList.AddRange(_gameFinishedScreenVisualElement.Query<Button>().ToList());
                _gameFinishedButton = _gameFinishedScreenVisualElement.Q<Button>("GameFinishedButton");
                if(_gameFinishedButton != null) _gameFinishedButton.clicked += OnGameFinishedButtonClicked;
                _gameFinishedScreenVisualElement.style.display = DisplayStyle.None;
            }

            _levelLoseScreenVisualElement = _rootVisualElement.Q<VisualElement>("LoseScreenVisualElement");

            if(_levelLoseScreenVisualElement != null)
            {
                _inGameUIButtonsList.AddRange(_levelLoseScreenVisualElement.Query<Button>().ToList());

                _loseQuitButton = _levelLoseScreenVisualElement.Q<Button>("QuitButton");
                if(_loseQuitButton != null) _loseQuitButton.clicked += OnQuitButtonClicked;

                _retryButton = _levelLoseScreenVisualElement.Q<Button>("RetryButton");
                if(_retryButton != null) _retryButton.clicked += OnRestartButtonClicked;

                _levelLoseScreenVisualElement.style.display = DisplayStyle.None;
            }

            _levelWinScreenVisualElement = _rootVisualElement.Q<VisualElement>("WinScreenVisualElement");

            if(_levelWinScreenVisualElement != null)
            {
                _inGameUIButtonsList.AddRange(_levelWinScreenVisualElement.Query<Button>().ToList());

                _continueButton = _levelWinScreenVisualElement.Q<Button>("ContinueButton");

                if(_continueButton != null) _continueButton.clicked += OnContinueButtonClicked;

                _levelWinScreenVisualElement.style.display = DisplayStyle.None;
            }
        }

        private void OnEnable()
        {
            ManagedEventBridgeSystem.OnEnergyValueChanged += OnEnergyValueChanged;
            ManagedEventBridgeSystem.OnGameFinished += OnGameFinished;
            ManagedEventBridgeSystem.OnHealthValueChanged += OnHealthValueChanged;
            ManagedEventBridgeSystem.OnLevelLost += OnLevelLost;
            ManagedEventBridgeSystem.OnLevelValueChanged += OnLevelValueChanged;
            ManagedEventBridgeSystem.OnLevelWon += OnLevelWon;
            ManagedEventBridgeSystem.OnLootTutorialStateChanged += OnLootTutorialStateChanged;
            ManagedEventBridgeSystem.OnPauseButtonClicked += OnPauseButtonClicked;
            ManagedEventBridgeSystem.OnPlayerDashCooldownStarted += OnPlayerDashCooldownStarted;
            ManagedEventBridgeSystem.OnQuitButtonClicked += OnQuitButtonClicked;
            ManagedEventBridgeSystem.OnRestartButtonClicked += OnRestartButtonClicked;
            ManagedEventBridgeSystem.OnResumeButtonClicked += OnResumeButtonClicked;
            ManagedEventBridgeSystem.OnTurretCooldownStarted += OnTurretCooldownStarted;
            ManagedEventBridgeSystem.OnTurretsTutorialStateChanged += OnTurretsTutorialStateChanged;
            ManagedEventBridgeSystem.OnWavePrepCountdownStarted += OnWavePrepCountdownStarted;
        }

        private void OnDisable()
        {
            ManagedEventBridgeSystem.OnEnergyValueChanged -= OnEnergyValueChanged;
            ManagedEventBridgeSystem.OnGameFinished -= OnGameFinished;
            ManagedEventBridgeSystem.OnHealthValueChanged -= OnHealthValueChanged;
            ManagedEventBridgeSystem.OnLevelLost -= OnLevelLost;
            ManagedEventBridgeSystem.OnLevelValueChanged -= OnLevelValueChanged;
            ManagedEventBridgeSystem.OnLevelWon -= OnLevelWon;
            ManagedEventBridgeSystem.OnLootTutorialStateChanged -= OnLootTutorialStateChanged;
            ManagedEventBridgeSystem.OnPauseButtonClicked -= OnPauseButtonClicked;
            ManagedEventBridgeSystem.OnPlayerDashCooldownStarted -= OnPlayerDashCooldownStarted;
            ManagedEventBridgeSystem.OnQuitButtonClicked -= OnQuitButtonClicked;
            ManagedEventBridgeSystem.OnRestartButtonClicked -= OnRestartButtonClicked;
            ManagedEventBridgeSystem.OnResumeButtonClicked -= OnResumeButtonClicked;
            ManagedEventBridgeSystem.OnTurretCooldownStarted -= OnTurretCooldownStarted;
            ManagedEventBridgeSystem.OnTurretsTutorialStateChanged -= OnTurretsTutorialStateChanged;
            ManagedEventBridgeSystem.OnWavePrepCountdownStarted -= OnWavePrepCountdownStarted;
        }

        private void Update()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            if(world == null || !world.IsCreated) return;
            if(_entityManager.World != world) { RefreshEcsReferences(); }

            foreach(var button in _inGameUIButtonsList) { Pulse(button); }

            Pulse(_currentBlinkingVisualElement);
        }

        #endregion

        #region Button Event Callbacks

        private void OnContinueButtonClicked()
        {
            if(_levelWinScreenVisualElement != null) { _levelWinScreenVisualElement.style.display = DisplayStyle.None; }

            _pauseButton.SetEnabled(true);

            if(!_levelWonQuery.IsEmpty)
            {
                Entity spawnerEntity = _entityManager.CreateEntityQuery(typeof(EnemySpawnerTag)).GetSingletonEntity();
                _entityManager.SetComponentEnabled<LevelWonTag>(spawnerEntity , false);
            }

            _entityManager.CreateEntity(typeof(AdvanceLevelEventTag));
            _entityManager.CreateEntity(typeof(ResumeInputTag));
        }

        private void OnGameFinishedButtonClicked()
        {
            if(_gameFinishedScreenVisualElement != null) { _gameFinishedScreenVisualElement.style.display = DisplayStyle.None; }

            _pauseButton.SetEnabled(true);
            _entityManager.CreateEntity(typeof(ResumeInputTag));

            SceneManager.LoadScene("Game");
        }

        private void OnPauseButtonClicked()
        {
            _entityManager.CreateEntity(typeof(PauseInputTag));
            _pauseButton.SetEnabled(false);
            _pauseMenuVisualElement.style.display = DisplayStyle.Flex;
        }

        private void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnRestartButtonClicked() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

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

        private void OnGameFinished()
        {
            if(!_entityManager.World.IsCreated) return;

            if(_gameFinishedScreenVisualElement != null && _gameFinishedScreenVisualElement.style.display == DisplayStyle.None)
            {
                _entityManager.CreateEntity(typeof(PauseInputTag));
                _pauseButton.SetEnabled(false);
                _gameFinishedScreenVisualElement.style.display = DisplayStyle.Flex;
            }
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

        private void OnLevelLost()
        {
            if(_levelLoseScreenVisualElement != null && _levelLoseScreenVisualElement.style.display == DisplayStyle.None)
            {
                _entityManager.CreateEntity(typeof(PauseInputTag));
                _pauseButton.SetEnabled(false);
                _levelLoseScreenVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        private void OnLevelValueChanged(int currentLevel)
        {
            if(!_entityManager.World.IsCreated) return;
            _entityManager.CompleteDependencyBeforeRO<LevelComponent>();
            if(!_levelQuery.IsEmptyIgnoreFilter) _levelValueLabel.text = $"{currentLevel:F0}";
        }

        private void OnLevelWon()
        {
            if(!_entityManager.World.IsCreated) return;

            bool isGameFinished = false;

            if(!_lastLevelQuery.IsEmptyIgnoreFilter && !_levelQuery.IsEmptyIgnoreFilter)
            {
                int currentLevel = _levelQuery.GetSingleton<LevelComponent>().Value;
                int lastLevel = _lastLevelQuery.GetSingleton<LastLevelComponent>().Value;
                if(currentLevel >= lastLevel) { isGameFinished = true; }
            }

            if(isGameFinished)
            {
                if(_gameFinishedScreenVisualElement != null && _gameFinishedScreenVisualElement.style.display == DisplayStyle.None)
                {
                    _entityManager.CreateEntity(typeof(PauseInputTag));
                    _pauseButton.SetEnabled(false);
                    _gameFinishedScreenVisualElement.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                if(_levelWinScreenVisualElement != null && _levelWinScreenVisualElement.style.display == DisplayStyle.None)
                {
                    _entityManager.CreateEntity(typeof(PauseInputTag));
                    _pauseButton.SetEnabled(false);
                    _levelWinScreenVisualElement.style.display = DisplayStyle.Flex;
                }
            }
        }

        private void OnLootTutorialStateChanged(bool isActive)
        {
            if(isActive)
            {
                if(_lootTutorialLabel == null)
                {
                    _lootTutorialLabel = new Label
                    {
                        style =
                        {
                            backgroundImage = new StyleBackground(hudPanelSprite) ,
                            unityBackgroundImageTintColor = wavePrepTimerLabelSpriteTintColour ,
                            backgroundColor = Color.clear ,
                            position = Position.Absolute ,
                            alignSelf = Align.Center ,
                            flexDirection = FlexDirection.Column ,
                            bottom = Length.Percent(tutorialLabelBottomPercent) ,
                            width = StyleKeyword.Auto ,
                            height = StyleKeyword.Auto ,
                            whiteSpace = WhiteSpace.Normal ,
                            paddingLeft = tutorialLabelPaddingLeft ,
                            paddingRight = tutorialLabelPaddingRight ,
                            paddingTop = tutorialLabelPaddingTop ,
                            paddingBottom = tutorialLabelPaddingBottom ,
                            unityTextAlign = TextAnchor.MiddleCenter ,
                            fontSize = tutorialLabelFontSize ,
                            color = Color.white ,
                            unityFontStyleAndWeight = FontStyle.Bold
                        }
                    };

                    var inlineRow = new VisualElement { style = { flexDirection = FlexDirection.Row , alignItems = Align.Center , justifyContent = Justify.Center } };
                    inlineRow.Add(new Label { text = lootTutorialTextP1 , style = { fontSize = tutorialLabelFontSize , color = Color.white , unityFontStyleAndWeight = FontStyle.Bold } });
                    inlineRow.Add(new VisualElement { style = { backgroundImage = new StyleBackground(lootTutorialTexture2D) , width = lootTutorialLabelIconSize , height = lootTutorialLabelIconSize , marginLeft = lootTutorialLabelIconMarginRight } });
                    _lootTutorialLabel.Add(inlineRow);

                    _lootTutorialLabel.Add(new Label { text = lootTutorialTextP2 , style = { fontSize = tutorialLabelFontSize , color = Color.white , unityFontStyleAndWeight = FontStyle.Bold , unityTextAlign = TextAnchor.MiddleCenter } });

                    _rootVisualElement.Add(_lootTutorialLabel);
                    _lootTutorialLabel.SendToBack();
                    AddVisualElementToPulse(_lootTutorialLabel);
                }
            }
            else
            {
                if(_lootTutorialLabel != null)
                {
                    _rootVisualElement.Remove(_lootTutorialLabel);
                    _lootTutorialLabel = null;
                }
            }
        }

        private void OnPlayerDashCooldownStarted(float timer , float3 worldPosition)
        {
            // Failsafe: if the player doesn't exist in memory yet, do nothing
            if(_playerQuery.IsEmptyIgnoreFilter) return;

            // Grab the entity locally to use as the dictionary key
            Entity entity = _playerQuery.GetSingletonEntity();

            if(timer <= zeroThreshold)
            {
                if(_playerDashCooldownLabelsDictionary.TryGetValue(entity , out Label label))
                {
                    _rootVisualElement.Remove(label);
                    _playerDashCooldownLabelsDictionary.Remove(entity);
                }

                return;
            }

            if(!_playerDashCooldownLabelsDictionary.TryGetValue(entity , out Label cooldownLabel))
            {
                cooldownLabel = new Label
                {
                    style =
                    {
                        backgroundColor = playerDashCooldownTimerLabelBgColour ,
                        borderBottomColor = playerDashCooldownTimerLabelBorderColour ,
                        borderBottomWidth = characterCooldownTimerLabelBorderWidth ,
                        borderLeftColor = playerDashCooldownTimerLabelBorderColour ,
                        borderLeftWidth = characterCooldownTimerLabelBorderWidth ,
                        borderRightColor = playerDashCooldownTimerLabelBorderColour ,
                        borderRightWidth = characterCooldownTimerLabelBorderWidth ,
                        borderTopColor = playerDashCooldownTimerLabelBorderColour ,
                        borderTopWidth = characterCooldownTimerLabelBorderWidth ,
                        color = playerDashCooldownTimerLabelTextColour ,
                        fontSize = characterCooldownTimerLabelFontSize ,
                        height = characterCooldownTimerLabelHeight ,
                        left = Length.Percent(characterCooldownTimerLabelAnchorPercent) ,
                        paddingBottom = characterCooldownTimerLabelPadding ,
                        paddingLeft = characterCooldownTimerLabelPadding ,
                        paddingRight = characterCooldownTimerLabelPadding ,
                        paddingTop = characterCooldownTimerLabelPadding ,
                        position = Position.Absolute ,
                        top = Length.Percent(characterCooldownTimerLabelAnchorPercent) ,
                        translate = new Translate(Length.Percent(characterCooldownTimerLabelTranslatePercentX) , Length.Percent(characterCooldownTimerLabelTranslatePercentY) , characterCooldownTimerLabelTranslatePercentZ) ,
                        unityFontStyleAndWeight = FontStyle.Bold ,
                        unityTextAlign = TextAnchor.MiddleCenter ,
                        width = characterCooldownTimerLabelWidth
                    }
                };

                _rootVisualElement.Add(cooldownLabel);
                cooldownLabel.SendToBack();
                _playerDashCooldownLabelsDictionary.Add(entity , cooldownLabel);
            }

            cooldownLabel.text = $"{timer:F0}"; //F0 means Integers only and F1 means float as well

            Vector2 screenPoint = RuntimePanelUtils.CameraTransformWorldToPanel(_rootVisualElement.panel , worldPosition , Camera.main);

            _entityManager.CompleteDependencyBeforeRO<ScreenBoundaryYComponent>();
            float boundaryY = _boundaryYQuery.GetSingleton<ScreenBoundaryYComponent>().Value;

            if(worldPosition.y >= boundaryY - characterCooldownTimerLabelFlipThreshold) { cooldownLabel.style.top = screenPoint.y + characterCooldownTimerLabelOffsetY; }
            else { cooldownLabel.style.top = screenPoint.y; }

            cooldownLabel.style.left = screenPoint.x;
        }

        private void OnTurretCooldownStarted(Entity entity , float timer , float3 worldPosition)
        {
            if(timer <= zeroThreshold)
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
                Color bgColor = strikerTurretCooldownTimerLabelBgColour;
                Color borderColor = strikerTurretCooldownTimerLabelBorderColour;

                if(_entityManager.HasComponent<ScatterTurretTag>(entity))
                {
                    bgColor = scatterTurretCooldownTimerLabelBgColour;
                    borderColor = scatterTurretCooldownTimerLabelBorderColour;
                }

                else if(_entityManager.HasComponent<BeamTurretTag>(entity))
                {
                    bgColor = beamTurretCooldownTimerLabelBgColour;
                    borderColor = beamTurretCooldownTimerLabelBorderColour;
                }

                cooldownLabel = new Label
                {
                    style =
                    {
                        backgroundColor = bgColor ,
                        borderBottomColor = borderColor ,
                        borderBottomWidth = characterCooldownTimerLabelBorderWidth ,
                        borderLeftColor = borderColor ,
                        borderLeftWidth = characterCooldownTimerLabelBorderWidth ,
                        borderRightColor = borderColor ,
                        borderRightWidth = characterCooldownTimerLabelBorderWidth ,
                        borderTopColor = borderColor ,
                        borderTopWidth = characterCooldownTimerLabelBorderWidth ,
                        fontSize = characterCooldownTimerLabelFontSize ,
                        height = characterCooldownTimerLabelHeight ,
                        left = Length.Percent(characterCooldownTimerLabelAnchorPercent) ,
                        paddingBottom = characterCooldownTimerLabelPadding ,
                        paddingLeft = characterCooldownTimerLabelPadding ,
                        paddingRight = characterCooldownTimerLabelPadding ,
                        paddingTop = characterCooldownTimerLabelPadding ,
                        position = Position.Absolute ,
                        top = Length.Percent(characterCooldownTimerLabelAnchorPercent) ,
                        translate = new Translate(Length.Percent(characterCooldownTimerLabelTranslatePercentX) , Length.Percent(characterCooldownTimerLabelTranslatePercentY) , characterCooldownTimerLabelTranslatePercentZ) ,
                        unityFontStyleAndWeight = FontStyle.Bold ,
                        unityTextAlign = TextAnchor.MiddleCenter ,
                        width = characterCooldownTimerLabelWidth
                    }
                };

                _rootVisualElement.Add(cooldownLabel);
                cooldownLabel.SendToBack();
                _turretCooldownLabelsDictionary.Add(entity , cooldownLabel);
            }

            cooldownLabel.text = $"{timer:F0}";

            Vector2 screenPoint = RuntimePanelUtils.CameraTransformWorldToPanel(_rootVisualElement.panel , worldPosition , Camera.main);

            _entityManager.CompleteDependencyBeforeRO<ScreenBoundaryYComponent>();
            float boundaryY = _boundaryYQuery.GetSingleton<ScreenBoundaryYComponent>().Value;

            if(worldPosition.y >= boundaryY - characterCooldownTimerLabelFlipThreshold) { cooldownLabel.style.top = screenPoint.y + characterCooldownTimerLabelOffsetY; }
            else { cooldownLabel.style.top = screenPoint.y; }

            cooldownLabel.style.left = screenPoint.x;
        }

        private void OnTurretsTutorialStateChanged(int currentLevel , bool isActive , int cost , string turretName , int turretType)
        {
            if(isActive)
            {
                if(_turretsTutorialLabel == null)
                {
                    _turretsTutorialLabel = new Label
                    {
                        style =
                        {
                            backgroundImage = new StyleBackground(hudPanelSprite) ,
                            unityBackgroundImageTintColor = wavePrepTimerLabelSpriteTintColour ,
                            backgroundColor = Color.clear ,
                            position = Position.Absolute ,
                            alignSelf = Align.Center ,
                            bottom = Length.Percent(tutorialLabelBottomPercent) ,
                            width = StyleKeyword.Auto ,
                            height = StyleKeyword.Auto ,
                            whiteSpace = WhiteSpace.NoWrap ,
                            paddingLeft = tutorialLabelPaddingLeft ,
                            paddingRight = tutorialLabelPaddingRight ,
                            paddingTop = tutorialLabelPaddingTop ,
                            paddingBottom = tutorialLabelPaddingBottom ,
                            unityTextAlign = TextAnchor.MiddleCenter ,
                            fontSize = tutorialLabelFontSize ,
                            color = Color.white ,
                            unityFontStyleAndWeight = FontStyle.Bold
                        }
                    };

                    _rootVisualElement.Add(_turretsTutorialLabel);
                    _turretsTutorialLabel.SendToBack();
                    AddVisualElementToPulse(_turretsTutorialLabel);
                }

                if(turretType == turretIdNone)
                {
                    if(currentLevel == tutorialLevel1) { _turretsTutorialLabel.text = level1TutorialText; }
                    else if(currentLevel == tutorialLevel2) { _turretsTutorialLabel.text = level2TutorialText; }
                    else if(currentLevel == tutorialLevel3) { _turretsTutorialLabel.text = level3TutorialText; }
                    else if(currentLevel == tutorialLevel4) { _turretsTutorialLabel.text = level4TutorialText; }
                }
                else { _turretsTutorialLabel.text = string.Format(turretsTutorialText , turretName , cost); }
            }
            else
            {
                if(_turretsTutorialLabel != null)
                {
                    _rootVisualElement.Remove(_turretsTutorialLabel);
                    _turretsTutorialLabel = null;
                }
            }
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
            int waveIndex = _waveIndexQuery.IsEmptyIgnoreFilter ? wave0Index : _waveIndexQuery.GetSingleton<WaveIndexComponent>().Value;

            string wavePrefix = waveIndex switch
            {
                Wave1Index => wave1Text ,
                Wave2Index => wave2Text ,
                _ => wave3Text
            };

            _wavePrepLabel.text = $"{wavePrefix} In\n{timer:F0}";
        }

        #endregion

        #region Custom Functions

        private void AddVisualElementToPulse(VisualElement visualElement) { _currentBlinkingVisualElement = visualElement; }

        private void Pulse(VisualElement visualElement)
        {
            if(visualElement == null) return;

            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + sineOffset) / sineDivisor;
            float alpha = Mathf.Lerp(minOpacity , maxOpacity , pulse);

            if(!visualElement.enabledSelf)
            {
                visualElement.style.opacity = zeroOpacity;
                return;
            }

            visualElement.style.opacity = alpha;
        }

        private void RefreshEcsReferences()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            if(world == null || !world.IsCreated) return;

            _entityManager = world.EntityManager;

            _boundaryYQuery = _entityManager.CreateEntityQuery(typeof(ScreenBoundaryYComponent));
            _energyQuery = _entityManager.CreateEntityQuery(typeof(CurrentEnergyComponent));
            _levelLostQuery = _entityManager.CreateEntityQuery(typeof(LevelLostTag));
            _levelWonQuery = _entityManager.CreateEntityQuery(typeof(LevelWonTag));
            _healthQuery = _entityManager.CreateEntityQuery(typeof(CurrentHealthComponent) , typeof(PlayerTag));
            _levelQuery = _entityManager.CreateEntityQuery(typeof(LevelComponent));
            _playerQuery = _entityManager.CreateEntityQuery(typeof(PlayerTag));
            _waveIndexQuery = _entityManager.CreateEntityQuery(typeof(WaveIndexComponent));

            _entityManager.CreateEntity(typeof(ResumeInputTag));
        }

        #endregion
    }
}