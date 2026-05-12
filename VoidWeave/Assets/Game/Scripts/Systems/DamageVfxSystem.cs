namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using Unity.Transforms;
    using System.Collections.Generic;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DamageVfxSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            // 1. Collect all the data we need into a simple list
            var vfxEntitiesList = new List<(Entity entityPrefab , LocalTransform localTransform , VfxColorComponent vfxColorComponent , VfxMeshComponent vfxMeshComponent , VfxScaleComponent vfxScaleComponent , VfxSizeComponent vfxSizeComponent , VfxTextureComponent vfxTextureComponent)>();

            foreach(var (damageVfxComponent , localTransform , vfxColorComponent , vfxMeshComponent , vfxScaleComponent , vfxSizeComponent , vfxTextureComponent , entity) in SystemAPI.Query<DamageVfxComponent , LocalTransform , VfxColorComponent , VfxMeshComponent , VfxScaleComponent , VfxSizeComponent , VfxTextureComponent>().WithAll<DamageTag>().WithEntityAccess())
            {
                vfxEntitiesList.Add((damageVfxComponent.Value , localTransform , vfxColorComponent , vfxMeshComponent , vfxScaleComponent , vfxSizeComponent , vfxTextureComponent));

                // Remove tag so we don't process this hit twice
                ecb.RemoveComponent<DamageTag>(entity);
            }

            // 2. Now that the Query loop is finished, we can safely make structural changes
            foreach(var vfxToSpawn in vfxEntitiesList)
            {
                // Instantiate immediately
                Entity entityInstance = systemState.EntityManager.Instantiate(vfxToSpawn.entityPrefab);

                // Set data immediately
                systemState.EntityManager.AddComponentData(entityInstance , vfxToSpawn.vfxColorComponent);
                systemState.EntityManager.AddComponentData(entityInstance, new VfxScaleComponent { Value = vfxToSpawn.vfxScaleComponent.Value });
                systemState.EntityManager.AddComponentData(entityInstance , vfxToSpawn.vfxSizeComponent);
                systemState.EntityManager.AddComponentData(entityInstance , new VfxUpdateTag());

                // 3. Attach the Mesh Object safely
                systemState.EntityManager.AddComponentObject(entityInstance , vfxToSpawn.vfxMeshComponent);
                systemState.EntityManager.AddComponentObject(entityInstance , vfxToSpawn.vfxTextureComponent);
                
                systemState.EntityManager.SetComponentData(entityInstance , vfxToSpawn.localTransform);
                systemState.EntityManager.SetComponentData(entityInstance , systemState.EntityManager.GetComponentData<LifetimeComponent>(vfxToSpawn.entityPrefab));
            }
        }
    }
}