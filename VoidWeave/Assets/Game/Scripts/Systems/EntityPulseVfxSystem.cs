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
        public void OnUpdate(ref SystemState state)
        {
            float time = (float)SystemAPI.Time.ElapsedTime;

            foreach(RefRW<LocalTransform> transform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PulseTag>())
            {
                float scale = 1.0f + math.sin(time * 10.0f) * 0.2f;
                transform.ValueRW.Scale = scale;
            }
        }
    }
}