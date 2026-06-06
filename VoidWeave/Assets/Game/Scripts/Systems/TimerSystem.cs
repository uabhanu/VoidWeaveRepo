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
            systemState.RequireForUpdate<NoActionComponent>();
            systemState.RequireForUpdate<TimerComponent>();
            systemState.RequireForUpdate<TimerExpiredComponent>();
            
            _tutorialActiveQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnerTag , TurretsTutorialActiveTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            int noAction = SystemAPI.GetSingleton<NoActionComponent>().Value;
            float timerExpired = SystemAPI.GetSingleton<TimerExpiredComponent>().Value;
            bool isTutorialActive = !_tutorialActiveQuery.IsEmpty;

            systemState.Dependency = new TimerJob { DeltaTime = SystemAPI.Time.DeltaTime , IsTutorialActive = isTutorialActive , NoAction = noAction , TimerExpired = timerExpired }.ScheduleParallel(systemState.Dependency);
        }
    }

    [BurstCompile]
    public partial struct TimerJob : IJobEntity
    {
        public float DeltaTime;
        public int NoAction;
        public bool IsTutorialActive;
        public float TimerExpired;

        private void Execute(ref TimerComponent timerComponent)
        {
            timerComponent.Value -= math.select(DeltaTime , NoAction , IsTutorialActive);
            timerComponent.Value = math.max(TimerExpired , timerComponent.Value);
        }
    }
}