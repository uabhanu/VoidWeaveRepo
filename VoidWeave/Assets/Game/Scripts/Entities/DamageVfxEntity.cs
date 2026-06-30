namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class DamageVfxEntity : MonoBehaviour
    {
        [SerializeField] private float lifetime;

        public class DamageVfxBaker : Baker<DamageVfxEntity>
        {
            public override void Bake(DamageVfxEntity authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new LifetimeComponent { Value = authoring.lifetime });
                AddComponent(entity , new VfxColorComponent());
                AddComponent(entity , new VfxScaleComponent());
                AddComponent(entity , new VfxSizeComponent());
                AddComponentObject(entity , new VfxMeshComponent());
                AddComponentObject(entity , new VfxTextureComponent());

                AddComponent(entity , new VfxUpdateTag());

                SetComponentEnabled<VfxUpdateTag>(entity , false);
            }
        }
    }
}