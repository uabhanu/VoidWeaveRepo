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
            float deltaTime = SystemAPI.Time.DeltaTime;
            float elapsedTime = (float)SystemAPI.Time.ElapsedTime;

            new InputMovementJob { DeltaTime = deltaTime }.ScheduleParallel();

            new BasicEnemyMovementJob { DeltaTime = deltaTime }.ScheduleParallel();
            new FastEnemyMovementJob { DeltaTime = deltaTime , ElapsedTime = elapsedTime }.ScheduleParallel();
            new SlowEnemyMovementJob { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    // --- PLAYER MOVEMENT ---
    [BurstCompile]
    [WithNone(typeof(EnemyTag))]
    public partial struct InputMovementJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform localTransform , in MoveSpeedComponent moveSpeedComponent , in MovementInputComponent movementInputComponent) { localTransform.Position.xy += movementInputComponent.Input * moveSpeedComponent.Speed * DeltaTime; }
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