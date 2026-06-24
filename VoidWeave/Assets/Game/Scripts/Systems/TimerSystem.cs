namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct TimerSystem : ISystem
    {
        private EntityQuery _tutorialActiveQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<TimerComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();

            _tutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag , TurretsTutorialActiveTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState) { systemState.Dependency = new TimerJob { DeltaTime = SystemAPI.Time.DeltaTime , IsTutorialActive = !_tutorialActiveQuery.IsEmpty , TimerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value }.ScheduleParallel(systemState.Dependency); }
    }

    [BurstCompile]
    public partial struct TimerJob : IJobEntity
    {
        public float DeltaTime;
        public bool IsTutorialActive;
        public float TimerExpired;

        private void Execute(ref TimerComponent timerComponent)
        {
            timerComponent.Value -= math.select(DeltaTime , 0 , IsTutorialActive);
            timerComponent.Value = math.max(TimerExpired , timerComponent.Value);
        }
    }
}