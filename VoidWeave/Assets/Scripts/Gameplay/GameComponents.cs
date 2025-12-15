namespace Gameplay
{
    using Unity.Entities;
    using Unity.Mathematics;

    #region Components

    public struct BaseMoveSpeedComponent : IComponentData
    {
        public float BaseSpeed;
    }

    public struct BulletEntityComponent : IComponentData
    {
        public Entity BulletEntity;
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
    
    public struct EnemyEntityComponent : IComponentData
    {
        public Entity EnemyEntity;
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
    
    public struct LootAmountComponent : IComponentData
    {
        public int LootAmount;
    }
    
    public struct LootEntityComponent : IComponentData
    {
        public Entity LootEntity;
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

    public struct TurretEntityComponent : IComponentData
    {
        public Entity TurretEntity;
    }

    public struct TurretRangeComponent : IComponentData
    {
        public float Range;
    }
    
    // Tracks the current wave number (1, 2, 3...)
    public struct WaveIndexComponent : IComponentData
    {
        public int WaveIndex;
    }
    
    // 0 = Preparation Phase, 1 = Combat Phase
    public struct WaveStateComponent : IComponentData
    {
        public int WaveState;
    }
    
    // How many enemies are left to spawn in the current wave
    public struct WaveStockComponent : IComponentData
    {
        public int WaveStock;
    }
    
    // Timer for the current phase (e.g., 30s prep time)
    public struct WaveTimerComponent : IComponentData
    {
        public float WaveTimer;
    }

    #endregion

    #region Tags

    public struct EnemySpawnerTag : IComponentData {}
    
    public struct LootPickupTag : IComponentData {}
    
    public struct PlayerTag : IComponentData {}
    
    public struct ProjectileTag : IComponentData {}

    public struct SeekerTag : IComponentData {}
    
    public struct StrikerTurretTag : IComponentData {}
    
    public struct TargetTag : IComponentData {}
    
    public struct TeamComponent : IComponentData { public int ID; } // 0 = Player , 1 = Enemy and so on
    
    public struct TurretTargetTag : IComponentData {}

    #endregion
}