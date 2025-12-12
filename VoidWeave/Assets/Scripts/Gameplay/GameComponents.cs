namespace Gameplay
{
    using Unity.Entities;
    using Unity.Mathematics;

    #region Components

    public struct BaseMoveSpeedComponent : IComponentData
    {
        public float BaseSpeed;
    }

    public struct BulletPrefabComponent : IComponentData
    {
        public Entity BulletPrefab;
    }

    public struct CurrentEnergyComponent : IComponentData
    {
        public int Energy;
    }

    public struct DashCooldownComponent : IComponentData
    {
        public float Timer;
    }

    public struct DashDurationComponent : IComponentData
    {
        public float Timer;
    }

    public struct DashInputComponent : IComponentData
    {
        public float IsPressed;
    }
    
    public struct EnemyPrefabComponent : IComponentData
    {
        public Entity EnemyPrefab;
    }
    
    public struct EnemySpawnRadiusComponent : IComponentData
    {
        public float EnemySpawnRadius;
    }

    public struct EnemySpawnRateComponent : IComponentData
    {
        public float EnemySpawnRate;
    }
    
    public struct EnemySpawnTimerComponent : IComponentData
    {
        public float EnemySpawnTimer;
    }

    public struct MoveSpeedComponent : IComponentData
    {
        public float MoveSpeed;
    }

    public struct MovementInputComponent : IComponentData
    {
        public float2 MoveInput;
    }
    
    public struct ProjectileDamageComponent : IComponentData
    {
        public float Damage;
    }

    public struct ProjectileLifetimeComponent : IComponentData
    {
        public float Timer;
    }
    
    public struct RandomComponent : IComponentData
    {
        public Random RandomValue;
    }
    
    public struct TargetPositionComponent : IComponentData
    {
        public float3 TargetPosition;
    }

    public struct TurretCooldownComponent : IComponentData
    {
        public float Timer;
    }

    public struct TurretDamageComponent : IComponentData
    {
        public float Damage;
    }

    public struct TurretDeploymentCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct TurretDeploymentInputComponent : IComponentData
    {
        public float IsPressed;
    }

    public struct TurretFireRateComponent : IComponentData
    {
        public float Rate;
    }

    public struct TurretPrefabComponent : IComponentData
    {
        public Entity TurretPrefab;
    }

    public struct TurretRangeComponent : IComponentData
    {
        public float Range;
    }

    #endregion

    #region Tags

    public struct EnemySpawnerTag : IComponentData {}
    
    public struct PlayerTag : IComponentData {}
    
    public struct ProjectileTag : IComponentData {}

    public struct SeekerTag : IComponentData {}
    
    public struct StrikerTurretTag : IComponentData {}
    
    public struct TargetTag : IComponentData {}
    
    public struct TeamComponent : IComponentData { public int ID; } // 0 = Player , 1 = Enemy and so on
    
    public struct TurretTargetTag : IComponentData {}

    #endregion
}