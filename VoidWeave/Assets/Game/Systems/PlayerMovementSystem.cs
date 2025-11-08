namespace Game.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct PlayerMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            if(!SystemAPI.TryGetSingleton<InputDataComponent>(out var inputDataComponent)) { return; }

            float2 movementInput = inputDataComponent.MovementInput;
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach(var (localTransform , playerMovementComponent , dashComponent) in SystemAPI.Query<RefRW<LocalTransform> , RefRO<PlayerMovementComponent> , RefRO<DashComponent>>().WithAll<PlayerTag>())
            {
                if(dashComponent.ValueRO.IsDashing) { continue; }

                float2 velocity = movementInput;

                if(math.lengthsq(velocity) > 1f) { velocity = math.normalize(velocity); }

                velocity *= playerMovementComponent.ValueRO.MoveSpeed;

                float3 newPosition = localTransform.ValueRO.Position;
                newPosition.xy += velocity * deltaTime;
                localTransform.ValueRW.Position = newPosition;
            }
        }
    }
}