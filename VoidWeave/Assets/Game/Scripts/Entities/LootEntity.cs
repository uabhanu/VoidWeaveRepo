namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class LootEntity : MonoBehaviour
    {
        [SerializeField] private float lifetime;
        [SerializeField] private float pulseAmplitude;
        [SerializeField] private float pulseFrequency;
        [SerializeField] private float timeBeforeEntityPulse;
        
        private class LootBaker : Baker<LootEntity>
        {
            public override void Bake(LootEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new LifetimeComponent { Value = authoring.lifetime });
                AddComponent(entity , new LootAmountComponent());
                AddComponent(entity , new PulseAmplitudeComponent { Value = authoring.pulseAmplitude });
                AddComponent(entity , new PulseFrequencyComponent { Value = authoring.pulseFrequency });
                AddComponent(entity , new TimeBeforeEntityPulseComponent { Value = authoring.timeBeforeEntityPulse });
                
                AddComponent(entity , new LootPickupTag());
                AddComponent(entity , new LootTag());
                AddComponent(entity , new PulseTag());
                
                SetComponentEnabled<PulseTag>(entity , false);
            }
        }
    }
}