namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class LootEntity : MonoBehaviour
    {
        [SerializeField] private float lifetime;
        [SerializeField] private float timeBeforeEntityPulse;
        
        private class LootBaker : Baker<LootEntity>
        {
            public override void Bake(LootEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new LifetimeComponent { Value = authoring.lifetime });
                AddComponent(entity , new LootAmountComponent());
                AddComponent(entity , new TimeBeforeEntityPulseComponent { Value = authoring.timeBeforeEntityPulse });
                
                AddComponent(entity , new LootPickupTag());
                AddComponent(entity , new LootTag());
            }
        }
    }
}