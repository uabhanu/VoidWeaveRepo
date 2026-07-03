namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class DashVfxEntity : MonoBehaviour
    {
        [SerializeField] private float lifetime; 

        private class DashVfxBaker : Baker<DashVfxEntity>
        {
            public override void Bake(DashVfxEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new LifetimeComponent { Value = authoring.lifetime });
                
                AddComponent(entity , new VfxUpdateTag());
                
                SetComponentEnabled<VfxUpdateTag>(entity , false);
            }
        }
    }
}