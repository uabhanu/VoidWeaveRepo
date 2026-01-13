namespace Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    #region Components

    public struct AttackRateComponent : IComponentData
    {
        public float AttackRate;
    }

    public struct BaseMoveSpeedComponent : IComponentData
    {
        public float Speed;
    }

    public struct BeamTurretCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct BeamTurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct BeamTurretLevelComponent : IComponentData
    {
        public int Level;
    }

    public struct BulletEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct CooldownComponent : IComponentData
    {
        public float Timer;
    }

    public struct CurrentEnergyComponent : IComponentData
    {
        public int Energy;
    }

    public struct CurrentHealthComponent : IComponentData
    {
        public float CurrentHealth;
    }

    public struct DamageComponent : IComponentData
    {
        public float Damage;
    }

    public struct DamageEventComponent : IComponentData
    {
        public float Damage;
    }

    public struct DamageMultiplierComponent : IComponentData
    {
        public float DamageMultiplier;
    }

    public struct DashCooldownComponent : IComponentData
    {
        public float Timer;
    }

    public struct DashDurationComponent : IComponentData
    {
        public float Duration;
    }

    public struct DashMultiplierComponent : IComponentData
    {
        public float Multiplier;
    }

    public struct EnemiesKilledComponent : IComponentData
    {
        public int KillsCount;
    }

    public struct EnemiesToKillComponent : IComponentData
    {
        public int EnemiesToKill;
    }

    public struct EnemiesToKillIncrementComponent : IComponentData
    {
        public int EnemiesToKillIncrement;
    }

    public struct EnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct EnemyFireRateComponent : IComponentData
    {
        public float Rate;
    }

    public struct EnemyProjectilePrefabComponent : IComponentData
    {
        public Entity Prefab;
    }

    public struct EnemyReloadTimerComponent : IComponentData
    {
        public float Timer;
    }

    public struct EnemySpawnRadiusComponent : IComponentData
    {
        public float Radius;
    }

    public struct EnemySpawnRateComponent : IComponentData
    {
        public float Rate;
    }

    public struct HealthMultiplierComponent : IComponentData
    {
        public float HealthMultiplier;
    }

    public struct LevelComponent : IComponentData
    {
        public int Level;
    }

    public struct LineEnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct LootAmountComponent : IComponentData
    {
        public int Amount;
    }

    public struct LootEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct LootMultiplierComponent : IComponentData
    {
        public float LootMultiplier;
    }

    public struct MaxHealthComponent : IComponentData
    {
        public float MaxHealth;
    }

    public struct MaxTurretsComponent : IComponentData
    {
        public int MaxTurrets;
    }

    public struct MoveSpeedComponent : IComponentData
    {
        public float Speed;
    }

    public struct PlayerInputComponent : IComponentData
    {
        public uint SelectedInput;
    }

    // How many projectiles spawn per shot
    // Striker = 1, Scatter = 5
    public struct ProjectileCountComponent : IComponentData
    {
        public int Count;
    }

    public struct ProjectileLifetimeComponent : IComponentData
    {
        public float Timer;
    }

    public struct RandomComponent : IComponentData
    {
        public Random Random;
    }

    public struct RangeComponent : IComponentData
    {
        public float Range;
    }

    public struct ScatterTurretCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct ScatterTurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct ScatterTurretLevelComponent : IComponentData
    {
        public int Level;
    }

    public struct SelectedTurretCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct SelectedTurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    // The total angle of the spread in degrees
    // Striker = 0, Scatter = 30
    public struct SpreadComponent : IComponentData
    {
        public float Degrees;
    }

    public struct SquareEnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct StrikerTurretCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct StrikerTurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct StrikerTurretLevelComponent : IComponentData
    {
        public int Level;
    }

    public struct TargetPositionComponent : IComponentData
    {
        public float3 Position;
    }

    public struct TeamComponent : IComponentData
    {
        public int ID; // 0 = Player , 1 = Enemy and so on
    }

    public struct TimerComponent : IComponentData
    {
        public float Timer;
    }

    public struct TriangleEnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct UnlockedEnemiesComponent : IComponentData
    {
        public uint UnlockedEnemiesBitmask;
    }

    public struct UnlockedTurretsComponent : IComponentData
    {
        public uint UnlockedTurretsBitmask;
    }

    public struct VelocityComponent : IComponentData
    {
        public float2 Velocity;
    }

    // The starting number of enemies for Wave 1
    public struct WaveBaseEnemyCountComponent : IComponentData
    {
        public int Count;
    }

    // How many additional enemies are added each subsequent wave
    public struct WaveEnemyIncrementComponent : IComponentData
    {
        public int Count;
    }

    // Tracks the current wave number (1, 2, 3...)
    public struct WaveIndexComponent : IComponentData
    {
        public int Index;
    }

    // How long the preparation phase lasts in seconds (Reset Value)
    public struct WavePrepDurationComponent : IComponentData
    {
        public float Duration;
    }

    public struct WavesPerLevelComponent : IComponentData
    {
        public int WavesPerLevel;
    }

    // 0 = Preparation Phase, 1 = Combat Phase
    public struct WaveStateComponent : IComponentData
    {
        public int State;
    }

    // How many enemies are left to spawn in the current wave
    public struct WaveStockComponent : IComponentData
    {
        public int Stock;
    }

    #endregion

    #region Tags

    public struct BeamTurretTag : IComponentData {}

    public struct CanShootTag : IComponentData {}

    public struct CanMeleeAttackTag : IComponentData {}

    public struct DeathTag : IComponentData {}

    public struct EnemyJustSpawnedTag : IComponentData {}

    public struct EnemyTag : IComponentData {}

    public struct EnemySpawnerTag : IComponentData {}

    public struct LineEnemyTag : IComponentData {}

    public struct TriangleEnemyTag : IComponentData {}

    public struct HasTargetTag : IComponentData {}

    public struct LootPickupTag : IComponentData {}

    public struct PlayerTag : IComponentData {}

    public struct ProjectileTag : IComponentData {}

    public struct RestartTag : IComponentData {}

    public struct ScatterTurretTag : IComponentData {}

    public struct SquareEnemyTag : IComponentData {}

    public struct StrikerTurretTag : IComponentData {}

    public struct UpgradeBeamTurretTag : IComponentData {}

    public struct UpgradeMaxTurretsTag : IComponentData {}

    public struct UpgradeScatterTurretTag : IComponentData {}

    public struct UpgradeStrikerTurretTag : IComponentData {}

    #endregion
}