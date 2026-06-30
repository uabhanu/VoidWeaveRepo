namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TurretConfigEntityNamingSystem : ISystem
    {
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>(); }

        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);

            foreach((StrikerTurretTag _ , Entity entity) in SystemAPI.Query<StrikerTurretTag>().WithDisabled<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("StrikerTurretConfig"));
                ecb.SetComponentEnabled<TurretDebugNamedTag>(entity , true);
            }

            foreach((ScatterTurretTag _ , Entity entity) in SystemAPI.Query<ScatterTurretTag>().WithDisabled<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("ScatterTurretConfig"));
                ecb.SetComponentEnabled<TurretDebugNamedTag>(entity , true);
            }

            foreach((BeamTurretTag _ , Entity entity) in SystemAPI.Query<BeamTurretTag>().WithDisabled<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("BeamTurretConfig"));
                ecb.SetComponentEnabled<TurretDebugNamedTag>(entity , true);
            }
        }
    }
}