namespace Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<InputUpComponent>();
            systemState.RequireForUpdate<InputDownComponent>();
            systemState.RequireForUpdate<InputLeftComponent>();
            systemState.RequireForUpdate<InputNoneComponent>();
            systemState.RequireForUpdate<InputRightComponent>();
            
            systemState.RequireForUpdate<MovementActiveComponent>();
            systemState.RequireForUpdate<MovementNoneComponent>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            float elapsedTime = (float)SystemAPI.Time.ElapsedTime;
            
            uint inputUp = SystemAPI.GetSingleton<InputUpComponent>().InputUpValue;
            uint inputDown = SystemAPI.GetSingleton<InputDownComponent>().InputDownValue;
            uint inputLeft = SystemAPI.GetSingleton<InputLeftComponent>().InputLeftValue;
            uint inputNone = SystemAPI.GetSingleton<InputNoneComponent>().InputNoneValue;
            uint inputRight = SystemAPI.GetSingleton<InputRightComponent>().InputRightValue;
            
            float movementActive = SystemAPI.GetSingleton<MovementActiveComponent>().MovementActive;
            float movementNone = SystemAPI.GetSingleton<MovementNoneComponent>().MovementNone;
            
            new BasicEnemyMovementJob { DeltaTime = deltaTime }.ScheduleParallel();
            new FastEnemyMovementJob { DeltaTime = deltaTime , ElapsedTime = elapsedTime , MovementNone = movementNone}.ScheduleParallel();
            new InputMovementJob { DeltaTime = deltaTime , InputDown = inputDown , InputLeft = inputLeft , InputNone = inputNone , InputRight = inputRight , InputUp = inputUp , MovementActive = movementActive , MovementNone = movementNone}.ScheduleParallel();
            new ProjectileMovementJob { DeltaTime = deltaTime }.ScheduleParallel();
            new SlowEnemyMovementJob { DeltaTime = deltaTime , MovementActive = movementActive , MovementNone = movementNone }.ScheduleParallel();
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
        public uint InputNone;
        public uint InputRight;
        public uint InputUp;
        public float MovementActive;
        public float MovementNone;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in PlayerInputComponent playerInputComponent)
        {
            uint selectedInput = playerInputComponent.PlayerInput;
            
            float down = math.select(MovementNone , MovementActive , (selectedInput & InputDown) != InputNone);
            float left = math.select(MovementNone , MovementActive , (selectedInput & InputLeft) != InputNone);
            float right = math.select(MovementNone , MovementActive , (selectedInput & InputRight) != InputNone);
            float up = math.select(MovementNone , MovementActive , (selectedInput & InputUp) != InputNone);

            // Construct Vector
            float2 input = new float2(right - left , up - down);

            // Apply Movement
            localTransform.Position.xy += input * moveSpeedComponent.Speed * DeltaTime;
        }
    }

    // --- BASIC ENEMY (Standard Chase) ---
    [BurstCompile]
    [WithAll(typeof(LineEnemyTag))]
    public partial struct BasicEnemyMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            float3 direction = math.normalizesafe(targetPositionComponent.Position - localTransform.Position);
            localTransform.Position.xy += direction.xy * moveSpeedComponent.Speed * DeltaTime;
        }
    }

    // --- FAST ENEMY (Zig-Zag / Evasive) ---
    [BurstCompile]
    [WithAll(typeof(TriangleEnemyTag))]
    public partial struct FastEnemyMovementJob : IJobEntity
    {
        public float DeltaTime;
        public float ElapsedTime;
        public float MovementNone;

        private void Execute(ref LocalTransform localTransform , in MovementZigZagAmplitudeComponent movementZigZagAmplitudeComponent , in MovementZigZagFrequencyComponent movementZigZagFrequencyComponent , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            // Calculate base direction
            float3 direction = math.normalizesafe(targetPositionComponent.Position - localTransform.Position);

            // Calculate perpendicular vector (Tangent) for the Zig-Zag offset
            // Rotates direction by 90 degrees in 2D: (x, y) -> (-y, x)
            float3 tangent = new float3(-direction.y , direction.x , MovementNone);

            // Apply Sine Wave to Tangent
            // Frequency = 10f (Speed of wiggle), Amplitude = 2.0f (Width of wiggle)
            float sineOffset = math.sin(ElapsedTime * movementZigZagFrequencyComponent.ZigZagFrequency) * movementZigZagAmplitudeComponent.ZigZagAmplitude;

            // Combine Forward + Sideways Movement
            localTransform.Position.xy += (direction.xy + tangent.xy * sineOffset) * moveSpeedComponent.Speed * DeltaTime;
        }
    }

    [BurstCompile]
    [WithAll(typeof(ProjectileTag))]
    public partial struct ProjectileMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in VelocityComponent velocityComponent)
        {
            localTransform.Position.xy += velocityComponent.Velocity * moveSpeedComponent.Speed * DeltaTime;
        }
    }

    // --- SLOW ENEMY
    [BurstCompile]
    [WithAll(typeof(SquareEnemyTag))]
    public partial struct SlowEnemyMovementJob : IJobEntity
    {
        public float DeltaTime;
        public float MovementActive;
        public float MovementNone;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in RangeComponent rangeComponent , in TargetPositionComponent targetPositionComponent)
        {
            float distanceSq = math.distancesq(localTransform.Position , targetPositionComponent.Position);
            float rangeSq = rangeComponent.Range * rangeComponent.Range;

            // Logic: If DistanceSq > RangeSq, we move (1). If DistanceSq <= RangeSq, we stop (0).
            // This prevents moving while reloading.
            float shouldMove = math.select(MovementActive , MovementNone , distanceSq <= rangeSq);

            float3 direction = math.normalizesafe(targetPositionComponent.Position - localTransform.Position);
            localTransform.Position.xy += direction.xy * moveSpeedComponent.Speed * DeltaTime * shouldMove;
        }
    }
}