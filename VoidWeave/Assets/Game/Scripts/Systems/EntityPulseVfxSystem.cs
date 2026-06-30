namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct EntityPulseVfxSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            foreach(var (localTransform , pulseAmplitude , pulseFrequency) in SystemAPI.Query<RefRW<LocalTransform> , RefRO<PulseAmplitudeComponent> , RefRO<PulseFrequencyComponent>>().WithAll<PulseTag>())
            {
                float scale = 1.0f + math.sin((float)SystemAPI.Time.ElapsedTime * pulseFrequency.ValueRO.Value) * pulseAmplitude.ValueRO.Value;
                localTransform.ValueRW.Scale = scale;
            }
        }
    }
}