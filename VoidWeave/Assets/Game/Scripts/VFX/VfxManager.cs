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

            _vfxQuery = _entityManager.CreateEntityQuery(typeof(VfxColorComponent) , typeof(VfxMeshComponent) , typeof(VfxScaleComponent) , typeof(VfxSizeComponent) , typeof(VfxTextureComponent) , typeof(VfxUpdateTag));
        }

        private void Update()
        {
            if(_vfxQuery.IsEmptyIgnoreFilter) return;

            _entityManager.CompleteDependencyBeforeRO<VfxColorComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxMeshComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxScaleComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxSizeComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxTextureComponent>();
            _entityManager.CompleteDependencyBeforeRO<VfxUpdateTag>();

            var entities = _vfxQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach(var entity in entities)
            {
                if(_entityManager.HasComponent<VisualEffect>(entity))
                {
                    float3 colorValue = _entityManager.GetComponentData<VfxColorComponent>(entity).Value;
                    Mesh meshValue = _entityManager.GetComponentData<VfxMeshComponent>(entity).Value;
                    float3 scaleValue = _entityManager.GetComponentData<VfxScaleComponent>(entity).Value;
                    float sizeValue = _entityManager.GetComponentData<VfxSizeComponent>(entity).Value;
                    Texture2D textureValue = _entityManager.GetComponentObject<VfxTextureComponent>(entity).Value;
                    var visualEffect = _entityManager.GetComponentObject<VisualEffect>(entity);

                    visualEffect.SetVector3("Color" , colorValue);
                    visualEffect.SetVector3("MainScale" , scaleValue);
                    visualEffect.SetMesh("MainMesh" , meshValue);
                    visualEffect.SetFloat("Size" , sizeValue);
                    visualEffect.SetTexture("MainTexture" , textureValue);
                }

                _entityManager.RemoveComponent<VfxUpdateTag>(entity);
            }

            entities.Dispose();
        }
    }
}