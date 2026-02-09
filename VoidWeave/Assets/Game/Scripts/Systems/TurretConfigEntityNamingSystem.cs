namespace Game.Scripts.Systems
{
    using Game.Scripts.Components;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TurretConfigEntityNamingSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach((StrikerTurretTag _ , Entity entity) in SystemAPI.Query<StrikerTurretTag>().WithNone<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("StrikerTurretConfig"));
                ecb.AddComponent<TurretDebugNamedTag>(entity);
            }

            foreach((ScatterTurretTag _ , Entity entity) in SystemAPI.Query<ScatterTurretTag>().WithNone<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("ScatterTurretConfig"));
                ecb.AddComponent<TurretDebugNamedTag>(entity);
            }

            foreach((BeamTurretTag _ , Entity entity) in SystemAPI.Query<BeamTurretTag>().WithNone<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("BeamTurretConfig"));
                ecb.AddComponent<TurretDebugNamedTag>(entity);
            }
        }
    }
}