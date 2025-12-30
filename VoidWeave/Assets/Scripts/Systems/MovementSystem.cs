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
        public void OnUpdate(ref SystemState state)
        {
            new InputMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
            new BasicEnemyMovementJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
            new FastEnemyMovementJob { DeltaTime = SystemAPI.Time.DeltaTime , ElapsedTime = (float)SystemAPI.Time.ElapsedTime }.ScheduleParallel();
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

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in PlayerInputComponent playerInputComponent)
        {
            uint selectedInput = playerInputComponent.SelectedInput;

            // Decode Bits to Directions (No Ifs)
            // 1=Up, 2=Down, 4=Left, 8=Right
            float up = math.select(0f , 1f , (selectedInput & 1) != 0);
            float down = math.select(0f , 1f , (selectedInput & 2) != 0);
            float left = math.select(0f , 1f , (selectedInput & 4) != 0);
            float right = math.select(0f , 1f , (selectedInput & 8) != 0);

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

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in TargetPositionComponent targetPositionComponent)
        {
            // Calculate base direction
            float3 direction = math.normalizesafe(targetPositionComponent.Position - localTransform.Position);

            // Calculate perpendicular vector (Tangent) for the Zig-Zag offset
            // Rotates direction by 90 degrees in 2D: (x, y) -> (-y, x)
            float3 tangent = new float3(-direction.y , direction.x , 0f);

            // Apply Sine Wave to Tangent
            // Frequency = 10f (Speed of wiggle), Amplitude = 2.0f (Width of wiggle)
            float sineOffset = math.sin(ElapsedTime * 10f) * 2.0f;

            // Combine Forward + Sideways Movement
            localTransform.Position.xy += (direction.xy + (tangent.xy * sineOffset)) * moveSpeedComponent.Speed * DeltaTime;
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

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in RangeComponent rangeComponent , in TargetPositionComponent targetPositionComponent)
        {
            float distanceSq = math.distancesq(localTransform.Position , targetPositionComponent.Position);
            float rangeSq = rangeComponent.Range * rangeComponent.Range;

            // Logic: If DistanceSq > RangeSq, we move (1). If DistanceSq <= RangeSq, we stop (0).
            // This prevents moving while reloading.
            float shouldMove = math.select(1f , 0f , distanceSq <= rangeSq);

            float3 direction = math.normalizesafe(targetPositionComponent.Position - localTransform.Position);
            localTransform.Position.xy += direction.xy * moveSpeedComponent.Speed * DeltaTime * shouldMove;
        }
    }
}