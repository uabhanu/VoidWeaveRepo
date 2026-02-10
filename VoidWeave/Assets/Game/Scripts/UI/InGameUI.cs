namespace Game.Scripts.UI
{
    using Components;
    using Systems;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private Color energyBarColor = Color.yellow;
        [SerializeField] private Color healthBarColor = Color.green;
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            if(!uiDocument) uiDocument = GetComponent<UIDocument>();

            var rootVisualElement = uiDocument.rootVisualElement;
            
            var energyLabel = rootVisualElement.Q<Label>("EnergyLabel");
            var healthLabel = rootVisualElement.Q<Label>("HealthLabel");
            
            energyLabel.style.backgroundColor = energyBarColor;
            healthLabel.style.backgroundColor = healthBarColor;

            var defaultGameObjectInjectionWorld = World.DefaultGameObjectInjectionWorld;
            var inGameUISystem = defaultGameObjectInjectionWorld.GetExistingSystemManaged<InGameUISystem>();

            inGameUISystem.SetReferences(energyLabel , healthLabel);

            var entity = defaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            defaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity , new UIReadyComponent { Value = true });
        }
    }
}