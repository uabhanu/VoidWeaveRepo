namespace Game.Scripts.UI
{
    using Components;
    using Systems;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Color healthBarColor = Color.green;

        private void Start()
        {
            if(!uiDocument) uiDocument = GetComponent<UIDocument>();

            var rootVisualElement = uiDocument.rootVisualElement;
            var healthBarFillVisualElement = rootVisualElement.Q<VisualElement>("HealthBarFillVisualElement");
            var healthLabel = rootVisualElement.Q<Label>("HealthLabel");

            if(healthBarFillVisualElement == null || healthLabel == null) return;

            healthBarFillVisualElement.style.backgroundColor = healthBarColor;

            var defaultGameObjectInjectionWorld = World.DefaultGameObjectInjectionWorld;
            var inGameUISystem = defaultGameObjectInjectionWorld.GetExistingSystemManaged<InGameUISystem>();

            inGameUISystem.SetReferences(healthBarFillVisualElement , healthLabel);

            var entity = defaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            defaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity , new UIReadyComponent { Value = true });
        }
    }
}