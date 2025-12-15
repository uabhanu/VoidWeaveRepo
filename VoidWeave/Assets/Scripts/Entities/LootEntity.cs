namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class LootEntity : MonoBehaviour
    {
        [SerializeField] private int lootAmount;

        private class LootBaker : Baker<LootEntity>
        {
            public override void Bake(LootEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new LootAmountComponent { LootAmount = authoring.lootAmount });
                AddComponent(entity , new LootPickupTag());
            }
        }
    }
}