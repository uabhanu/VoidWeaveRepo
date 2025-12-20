namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class LootEntity : MonoBehaviour
    {
        private class LootBaker : Baker<LootEntity>
        {
            public override void Bake(LootEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new LootAmountComponent { Amount = 0 });
                AddComponent(entity , new LootPickupTag());
            }
        }
    }
}