namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<InputUpComponent>();
            systemState.RequireForUpdate<InputDownComponent>();
            systemState.RequireForUpdate<InputLeftComponent>();
            systemState.RequireForUpdate<InputRightComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            new BasicEnemyMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
            new FastEnemyMovementJob { DeltaTime = SystemAPI.Time.DeltaTime , ElapsedTime = (float)SystemAPI.Time.ElapsedTime }.ScheduleParallel();
            new InputMovementJob { DeltaTime = SystemAPI.Time.DeltaTime , InputDown = SystemAPI.GetSingleton<InputDownComponent>().Value , InputLeft = SystemAPI.GetSingleton<InputLeftComponent>().Value , InputRight = SystemAPI.GetSingleton<InputRightComponent>().Value , InputUp = SystemAPI.GetSingleton<InputUpComponent>().Value }.ScheduleParallel();
            new ProjectileMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
            new SlowEnemyMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
        }
    }

    // --- PLAYER MOVEMENT ---
    [BurstCompile]
    [WithNone(typeof(EnemyTag))]
    public partial struct InputMovementJob : IJobEntity
    {
        public float DeltaTime;
        public uint InputDown;
        public uint InputLeft;
        public uint InputRight;
        public uint InputUp;

        private void Execute(ref LocalTransform localTransform , ref MoveDirectionComponent moveDirectionComponent , in MoveSpeedComponent moveSpeedComponent , in PlayerInputComponent playerInputComponent)
        {
            uint selectedInput = playerInputComponent.Value;

            float down = math.select(0 , 1 , (selectedInput & InputDown) != 0);
            float left = math.select(0 , 1 , (selectedInput & InputLeft) != 0);
            float right = math.select(0 , 1 , (selectedInput & InputRight) != 0);
            float up = math.select(0 , 1 , (selectedInput & InputUp) != 0);

            // Construct Vector
            var input = new float2(right - left , up - down);

            moveDirectionComponent.Value = math.select(new float3(input.x , input.y , 0) , moveDirectionComponent.Value , math.lengthsq(input) < 0.001f);

            // Apply Movement
            localTransform.Position.xy += input * moveSpeedComponent.Value * DeltaTime;
        }
    }

    // --- BASIC ENEMY (Standard Chase) ---
    [BurstCompile]
    public partial struct BasicEnemyMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(in LineEnemyComponent lineEnemyComponent , ref LocalTransform localTransform , ref MoveDirectionComponent moveDirectionComponent , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            float3 direction = math.normalizesafe(targetPositionComponent.Value - localTransform.Position);
            moveDirectionComponent.Value = direction;
            int isLineEnemy = lineEnemyComponent.Value;
            localTransform.Position.xy += direction.xy * moveSpeedComponent.Value * DeltaTime * isLineEnemy;
        }
    }

    // --- FAST ENEMY (Zig-Zag / Evasive) ---
    [BurstCompile]
    public partial struct FastEnemyMovementJob : IJobEntity
    {
        public float DeltaTime;
        public float ElapsedTime;

        private void Execute(ref LocalTransform localTransform , ref MoveDirectionComponent moveDirectionComponent , in MovementZigZagAmplitudeComponent movementZigZagAmplitudeComponent , in MovementZigZagFrequencyComponent movementZigZagFrequencyComponent , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent , in TriangleEnemyComponent triangleEnemyComponent)
        {
            // Calculate base direction
            float3 direction = math.normalizesafe(targetPositionComponent.Value - localTransform.Position);

            int isTriangleEnemy = triangleEnemyComponent.Value;

            // Calculate perpendicular vector (Tangent) for the Zig-Zag offset
            // Rotates direction by 90 degrees in 2D: (x, y) -> (-y, x)
            var tangent = new float3(-direction.y , direction.x , 0);

            // Apply Sine Wave to Tangent
            // Frequency = 10f (Speed of wiggle), Amplitude = 2.0f (Width of wiggle)
            float sineOffset = math.sin(ElapsedTime * movementZigZagFrequencyComponent.Value) * movementZigZagAmplitudeComponent.Value;

            float2 velocity2D = direction.xy + tangent.xy * sineOffset;
            float3 velocity = new float3(velocity2D , 0);
            moveDirectionComponent.Value = math.normalizesafe(velocity);

            // Combine Forward + Sideways Movement
            localTransform.Position.xy += (direction.xy + tangent.xy * sineOffset) * moveSpeedComponent.Value * DeltaTime * isTriangleEnemy;
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileTag))]
    public partial struct ProjectileMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in VelocityComponent velocityComponent) { localTransform.Position.xy += velocityComponent.Value * moveSpeedComponent.Value * DeltaTime; }
    }

    // --- SLOW ENEMY
    [BurstCompile]
    [WithAll(typeof(SquareEnemyTag))]
    public partial struct SlowEnemyMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , ref MoveDirectionComponent moveDirectionComponent , in MoveSpeedComponent moveSpeedComponent , in RangeComponent rangeComponent , in TargetPositionComponent targetPositionComponent)
        {
            float distanceSq = math.distancesq(localTransform.Position , targetPositionComponent.Value);
            float rangeSq = rangeComponent.Value * rangeComponent.Value;

            // Logic: If DistanceSq > RangeSq, we move (1). If DistanceSq <= RangeSq, we stop (0).
            // This prevents moving while reloading.
            float shouldMove = math.select(1 , 0 , distanceSq <= rangeSq);

            float3 direction = math.normalizesafe(targetPositionComponent.Value - localTransform.Position);
            moveDirectionComponent.Value = direction;
            localTransform.Position.xy += direction.xy * moveSpeedComponent.Value * DeltaTime * shouldMove;
        }
    }
}