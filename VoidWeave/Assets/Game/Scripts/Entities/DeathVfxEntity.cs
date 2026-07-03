namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class DeathVfxEntity : MonoBehaviour
    {
        [SerializeField] private float lifetime;

        private class DeathVfxBaker : Baker<DeathVfxEntity>
        {
            public override void Bake(DeathVfxEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new LifetimeComponent { Value = authoring.lifetime });
                
                AddComponent(entity , new VfxUpdateTag());
                
                SetComponentEnabled<VfxUpdateTag>(entity , false);
            }
        }
    }
}