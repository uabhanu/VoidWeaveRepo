namespace Game.Scripts.UI
{
    using Components;
    using System.Collections.Generic;
    using Systems;
    using Unity.Entities;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class MainMenuUI : MonoBehaviour
    {
        #region Variables
        
        private Button _quitButton;
        private Button _startButton;
        private List<Button> _mainMenuUIButtonsList = new();
        
        private EntityManager _entityManager;
        
        private VisualElement _mainMenuVisualElement;
        private VisualElement _rootVisualElement;
        private VisualElement _scoresVisualElement;
        
        [SerializeField] private float maxOpacity;
        [SerializeField] private float minOpacity;
        [SerializeField] private float pulseSpeed;
        [SerializeField] private float sineDivisor;
        [SerializeField] private float sineOffset;
        
        [SerializeField] private UIDocument uiDocument;
        
        #endregion
        
        #region Unity Callbacks

        private void Start()
        {
            if(!uiDocument) { uiDocument = GetComponent<UIDocument>(); }

            _rootVisualElement = uiDocument.rootVisualElement;
            _mainMenuVisualElement = _rootVisualElement.Q<VisualElement>("MainMenuVisualElement");
            _scoresVisualElement = _rootVisualElement.Q<VisualElement>("ScoresVisualElement");
            
            _quitButton = _mainMenuVisualElement.Q<Button>("QuitButton");
            _quitButton.clicked += () => { GameEventsSystem.OnQuitButtonClicked?.Invoke(); };
            
            _startButton = _mainMenuVisualElement.Q<Button>("StartButton");
            _startButton.clicked += () => { GameEventsSystem.OnStartButtonClicked?.Invoke(); };
            
            _mainMenuUIButtonsList = _mainMenuVisualElement.Query<Button>().ToList();

            var world = World.DefaultGameObjectInjectionWorld;
            if(world != null) { _entityManager = world.EntityManager; }
        }

        private void Update()
        {
            if(_mainMenuVisualElement.style.display == DisplayStyle.None) return;

            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + sineOffset) / sineDivisor;
            float alpha = Mathf.Lerp(minOpacity , maxOpacity , pulse);

            foreach(var button in _mainMenuUIButtonsList)
            {
                if(button != null) { button.style.opacity = alpha; }
            }
        }

        private void OnEnable()
        {
            GameEventsSystem.OnQuitButtonClicked += OnQuitButtonClicked;
            GameEventsSystem.OnStartButtonClicked += OnStartButtonClicked;
        }

        private void OnDisable()
        {
            GameEventsSystem.OnQuitButtonClicked -= OnQuitButtonClicked;
            GameEventsSystem.OnStartButtonClicked -= OnStartButtonClicked;
        }
        
        #endregion
        
        #region Button Event Callbacks
        
        private void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #else
				Application.Quit();
            #endif
        }

        private void OnStartButtonClicked()
        {
            _entityManager.CreateEntity(typeof(StartGameRequestTag));

            _mainMenuVisualElement.style.display = DisplayStyle.None;
            _scoresVisualElement.style.display = DisplayStyle.Flex;
        }
        
        #endregion
    }
}