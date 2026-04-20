namespace Game.Scripts.UI
{
    using Components;
    using System.Collections.Generic;
    using Systems;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class MainMenuUI : MonoBehaviour
    {
        #region Variables
        
        private EntityManager _entityManager;
        private VisualElement _mainMenuVisualElement;
        private VisualElement _rootVisualElement;
        private VisualElement _scoresVisualElement;
        private Button _startButton;
        private List<Button> _mainMenuUIButtonsList = new();
        
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
            _startButton = _rootVisualElement.Q<Button>("StartButton");
            
            _mainMenuUIButtonsList = _mainMenuVisualElement.Query<Button>().ToList();

            _startButton.clicked += () => { GameEventsSystem.OnStartButtonClicked?.Invoke(); };

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

        private void OnEnable() { GameEventsSystem.OnStartButtonClicked += OnStartButtonClicked; }

        private void OnDisable() { GameEventsSystem.OnStartButtonClicked -= OnStartButtonClicked; }
        
        #endregion
        
        #region Button Event Callbacks

        private void OnStartButtonClicked()
        {
            _entityManager.CreateEntity(typeof(StartGameRequestTag));

            _mainMenuVisualElement.style.display = DisplayStyle.None;
            _scoresVisualElement.style.display = DisplayStyle.Flex;
        }
        
        #endregion
    }
}