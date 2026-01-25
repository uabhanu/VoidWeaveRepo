using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TurretConfigEntityNamingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState) { systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(systemState.WorldUnmanaged);
            
            foreach(var (_ , entity) in SystemAPI.Query<StrikerTurretTag>().WithNone<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("StrikerTurretConfig"));
                ecb.AddComponent<TurretDebugNamedTag>(entity);
            }
            
            foreach(var (_ , entity) in SystemAPI.Query<ScatterTurretTag>().WithNone<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("ScatterTurretConfig"));
                ecb.AddComponent<TurretDebugNamedTag>(entity);
            }
            
            foreach(var (_ , entity) in SystemAPI.Query<BeamTurretTag>().WithNone<TurretDebugNamedTag>().WithEntityAccess())
            {
                ecb.SetName(entity , new FixedString64Bytes("BeamTurretConfig"));
                ecb.AddComponent<TurretDebugNamedTag>(entity);
            }
        }
    }
}