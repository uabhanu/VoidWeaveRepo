namespace Game.Scripts.VFX
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.VFX;

    public class VfxManager : MonoBehaviour
    {
        private EntityManager _entityManager;
        private EntityQuery _vfxQuery;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;

            _vfxQuery = _entityManager.CreateEntityQuery(typeof(VfxColorComponent) , typeof(VfxSizeComponent) , typeof(VfxUpdateTag));
        }

        private void Update()
        {
            if(_vfxQuery.IsEmptyIgnoreFilter) return;

            _entityManager.CompleteDependencyBeforeRO<VfxColorComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxSizeComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxUpdateTag>();

            var entities = _vfxQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach(var entity in entities)
            {
                if(_entityManager.HasComponent<VisualEffect>(entity))
                {
                    var visualEffect = _entityManager.GetComponentObject<VisualEffect>(entity);
                    float3 colorValue = _entityManager.GetComponentData<VfxColorComponent>(entity).Value;
                    float sizeValue = _entityManager.GetComponentData<VfxSizeComponent>(entity).Value;

                    visualEffect.SetVector3("Color" , colorValue);
                    visualEffect.SetFloat("Size" , sizeValue);
                }

                _entityManager.RemoveComponent<VfxUpdateTag>(entity);
            }

            entities.Dispose();
        }
    }
}