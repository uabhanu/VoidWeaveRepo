namespace Game.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct DashSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            if(!SystemAPI.TryGetSingleton<InputDataComponent>(out var inputDataComponent)) { return; }

            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach(var (localTransform , dashComponentRef) in SystemAPI.Query<RefRW<LocalTransform> , RefRW<DashComponent>>().WithAll<PlayerTag>())
            {
                ref var dashComponent = ref dashComponentRef.ValueRW;

                UpdateCooldown(ref dashComponent , deltaTime);

                if(dashComponent.IsDashing) { ProcessDash(ref localTransform.ValueRW , ref dashComponent , deltaTime); }
                else { TryStartDash(ref dashComponent , inputDataComponent.DashPressed , inputDataComponent.MovementInput); }
            }
        }
        
        [BurstCompile]
        private static void ProcessDash(ref LocalTransform localTransform , ref DashComponent dashComponent , float deltaTime)
        {
            dashComponent.DashTimer += deltaTime;

            float dashSpeed = dashComponent.DashDistance / dashComponent.DashDuration;
            float3 newPosition = localTransform.Position;
            newPosition.xy += dashComponent.DashDirection * dashSpeed * deltaTime;
            localTransform.Position = newPosition;

            if(dashComponent.DashTimer >= dashComponent.DashDuration)
            {
                dashComponent.IsDashing = false;
                dashComponent.DashTimer = 0f;
                dashComponent.CurrentCooldown = dashComponent.CooldownDuration;
            }
        }

        [BurstCompile]
        private static void TryStartDash(ref DashComponent dashComponent , bool dashPressed , in float2 movementInput)
        {
            if(!dashPressed || dashComponent.CurrentCooldown > 0f) { return; }

            if(math.lengthsq(movementInput) > 0f)
            {
                dashComponent.DashDirection = math.normalize(movementInput);
                dashComponent.IsDashing = true;
                dashComponent.DashTimer = 0f;
            }
        }
        
        [BurstCompile]
        private static void UpdateCooldown(ref DashComponent dashComponent , float deltaTime)
        {
            if(dashComponent.CurrentCooldown > 0f) { dashComponent.CurrentCooldown -= deltaTime; }
        }
    }
}