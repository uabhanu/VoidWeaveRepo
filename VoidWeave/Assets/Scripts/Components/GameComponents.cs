namespace Components
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;

    #region Components

    public struct AttackRateComponent : IComponentData
    {
        public float AttackRate;
    }

    public struct BaseMoveSpeedComponent : IComponentData
    {
        public float Speed;
    }

    public struct BulletEntityComponent : IComponentData
    {
        public Entity Entity;
    }
    
    public struct CollisionRadiusComponent : IComponentData
    {
        public float Radius;
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
    
    public struct DashKeyComponent : IComponentData
    {
        public Key DashKey;
    }

    public struct DashMultiplierComponent : IComponentData
    {
        public float Multiplier;
    }
    
    public struct DeployKeyComponent : IComponentData
    {
        public Key DeployKey;
    }
    
    public struct DownKeyComponent : IComponentData
    {
        public Key DownKey;
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
    
    public struct EnemySpawnerEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct HealthMultiplierComponent : IComponentData
    {
        public float HealthMultiplier;
    }
    
    public struct InputDashValueComponent : IComponentData
    {
        public uint InputDashValue;
    }
    
    public struct InputDeployValueComponent : IComponentData
    {
        public uint InputDeployValue;
    }
    
    public struct InputDownValueComponent : IComponentData
    {
        public uint InputDownValue;
    }
    
    public struct InputEntityComponent : IComponentData
    {
        public Entity Entity;
    }
    
    public struct InputLeftValueComponent : IComponentData
    {
        public uint InputLeftValue;
    }
    
    public struct InputNoneValueComponent : IComponentData
    {
        public uint InputNoneValue;
    }
    
    public struct InputRightValueComponent : IComponentData
    {
        public uint InputRightValue;
    }
    
    public struct InputTurret1ValueComponent : IComponentData
    {
        public uint InputTurret1Value;
    }
    
    public struct InputTurret2ValueComponent : IComponentData
    {
        public uint InputTurret2Value;
    }
    
    public struct InputTurret3ValueComponent : IComponentData
    {
        public uint InputTurret3Value;
    }
    
    public struct InputUpValueComponent : IComponentData
    {
        public uint InputUpValue;
    }
    
    public struct LeftKeyComponent : IComponentData
    {
        public Key LeftKey;
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

    public struct MoveSpeedComponent : IComponentData
    {
        public float Speed;
    }
    
    public struct PlayerEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct PlayerInputComponent : IComponentData
    {
        public uint PlayerInput;
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
    
    public struct RightKeyComponent : IComponentData
    {
        public Key RightKey;
    }

    public struct SelectedTurretCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct SelectedTurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }
    
    public struct SpawnRadiusComponent : IComponentData
    {
        public float Radius;
    }

    public struct SpawnRateComponent : IComponentData
    {
        public float Rate;
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
    
    public struct Turret1KeyComponent : IComponentData
    {
        public Key Turret1Key;
    }
    
    public struct Turret2KeyComponent : IComponentData
    {
        public Key Turret2Key;
    }
    
    public struct Turret3KeyComponent : IComponentData
    {
        public Key Turret3Key;
    }
    
    public struct TurretConfigEntityComponent : IComponentData
    {
        public Entity Entity;
    }
    
    public struct TurretCostComponent : IComponentData
    {
        public int Cost;
    }

    public struct TurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct TurretLevelComponent : IComponentData
    {
        public int Level;
    }

    public struct UnlockedEnemiesComponent : IComponentData
    {
        public uint UnlockedEnemiesBitmask;
    }
    
    public struct UpKeyComponent : IComponentData
    {
        public Key UpKey;
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
    
    public struct InitializeGameTag : IComponentData {}

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
    
    public struct TurretDebugNamedTag : IComponentData {}

    public struct UpgradeBeamTurretTag : IComponentData {}

    public struct UpgradeMaxTurretsTag : IComponentData {}

    public struct UpgradeScatterTurretTag : IComponentData {}

    public struct UpgradeStrikerTurretTag : IComponentData {}

    #endregion
}