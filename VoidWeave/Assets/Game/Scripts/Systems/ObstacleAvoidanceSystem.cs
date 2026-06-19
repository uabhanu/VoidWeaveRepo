namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    [BurstCompile]
    public partial struct ObstacleAvoidanceSystem : ISystem
    {
        private EntityQuery _obstacleQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            _obstacleQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<CollisionRadiusComponent , LocalTransform>().WithNone<PlayerTag , ProjectileTag , TurretTag>().Build(ref systemState);

            systemState.RequireForUpdate<MinOverlapDistanceComponent>();
            systemState.RequireForUpdate<SeparationDistanceComponent>();
            systemState.RequireForUpdate<SeparationVelocityComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var obstaclePositionsNativeArray = _obstacleQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

            var obstacleAvoidanceJob = new ObstacleAvoidanceJob { DeltaTime = SystemAPI.Time.DeltaTime , MinOverlapDistance = SystemAPI.GetSingleton<MinOverlapDistanceComponent>().Value , ObstaclePositionsNativeArray = obstaclePositionsNativeArray , SeparationDistance = SystemAPI.GetSingleton<SeparationDistanceComponent>().Value , SeparationVelocity = SystemAPI.GetSingleton<SeparationVelocityComponent>().Value };

            systemState.Dependency = obstacleAvoidanceJob.ScheduleParallel(_obstacleQuery , systemState.Dependency);

            obstaclePositionsNativeArray.Dispose(systemState.Dependency);
        }

        [BurstCompile]
        public partial struct ObstacleAvoidanceJob : IJobEntity
        {
            public float DeltaTime;
            public float MinOverlapDistance;
            [ReadOnly] public NativeArray<LocalTransform> ObstaclePositionsNativeArray;
            public float SeparationDistance;
            public float SeparationVelocity;

            private void Execute(ref LocalTransform localTransform , in CollisionRadiusComponent collisionRadiusComponent)
            {
                float2 adjustment = float2.zero;

                for(int i = 0 ; i < ObstaclePositionsNativeArray.Length ; i++)
                {
                    float2 otherPos = ObstaclePositionsNativeArray[i].Position.xy;
                    float distSq = math.distancesq(localTransform.Position.xy , otherPos);

                    int isNotSelf = math.select(0 , 1 , distSq > MinOverlapDistance);

                    float combinedRadius = (collisionRadiusComponent.Value * localTransform.Scale + collisionRadiusComponent.Value * ObstaclePositionsNativeArray[i].Scale) * SeparationDistance;
                    int isOverlap = math.select(0 , 1 , distSq < (combinedRadius * combinedRadius));

                    float dist = math.sqrt(distSq);
                    float2 pushDir = math.normalizesafe(localTransform.Position.xy - otherPos);

                    adjustment += pushDir * (combinedRadius - dist) * isOverlap * isNotSelf * SeparationVelocity;
                }

                localTransform.Position.xy += adjustment * DeltaTime;
            }
        }
    }
}