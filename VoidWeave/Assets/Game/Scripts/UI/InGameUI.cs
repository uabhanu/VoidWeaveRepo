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
            
            var energyTextLabel = rootVisualElement.Q<Label>("EnergyTextLabel");
            var energyValueLabel = rootVisualElement.Q<Label>("EnergyValueLabel");
            
            var healthTextLabel = rootVisualElement.Q<Label>("HealthTextLabel");
            var healthValueLabel = rootVisualElement.Q<Label>("HealthValueLabel");
            
            energyTextLabel.style.backgroundColor = energyBarColor;
            energyValueLabel.style.backgroundColor = energyBarColor;
            
            healthTextLabel.style.backgroundColor = healthBarColor;
            healthValueLabel.style.backgroundColor = healthBarColor;

            var defaultGameObjectInjectionWorld = World.DefaultGameObjectInjectionWorld;
            var inGameUISystem = defaultGameObjectInjectionWorld.GetExistingSystemManaged<InGameUISystem>();

            inGameUISystem.SetReferences(energyValueLabel , healthValueLabel);

            var entity = defaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            defaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity , new UIReadyComponent { Value = true });
        }
    }
}