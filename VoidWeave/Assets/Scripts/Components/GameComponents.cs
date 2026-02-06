namespace Components
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;

    #region Components

    public struct ActiveWaveStateComponent : IComponentData
    {
        public int Value;
    }

    public struct AttackRateComponent : IComponentData
    {
        public float Value;
    }

    public struct BaseMoveSpeedComponent : IComponentData
    {
        public float Value;
    }
    
    public struct BeamTurretUnlockLevelComponent : IComponentData
    {
        public int Value;
    }
    
    public struct BoundaryOffsetComponent : IComponentData
    {
        public float Value;
    }

    public struct BulletEntityComponent : IComponentData
    {
        public Entity Entity;
    }
    
    public struct BulletRotationOffsetComponent : IComponentData
    {
        public float Value;
    }
    
    public struct CameraOrthographicSizeComponent : IComponentData
    {
        public float Value;
    }

    public struct CollisionActiveComponent : IComponentData
    {
        public int Value;
    }

    public struct CollisionNoneComponent : IComponentData
    {
        public int Value;
    }

    public struct CollisionRadiusComponent : IComponentData
    {
        public float Value;
    }

    public struct CooldownComponent : IComponentData
    {
        public float Value;
    }

    public struct CurrentEnergyComponent : IComponentData
    {
        public int Value;
    }

    public struct CurrentHealthComponent : IComponentData
    {
        public float Value;
    }

    public struct DamageComponent : IComponentData
    {
        public float Value;
    }

    public struct DamageEventComponent : IComponentData
    {
        public float Value;
    }

    public struct DamageMultiplierComponent : IComponentData
    {
        public float Value;
    }

    public struct DashCooldownComponent : IComponentData
    {
        public float Value;
    }
    
    public struct DashCooldownDefaultComponent : IComponentData
    {
        public float Value;
    }

    public struct DashDurationComponent : IComponentData
    {
        public float Value;
    }
    
    public struct DashDurationDefaultComponent : IComponentData
    {
        public float Value;
    }

    public struct DashKeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct DashMultiplierComponent : IComponentData
    {
        public float Value;
    }

    public struct DeployKeyComponent : IComponentData
    {
        public Key Value;
    }
    
    public struct DoActionComponent : IComponentData
    {
        public int Value;
    }

    public struct DownKeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct EnemiesKilledComponent : IComponentData
    {
        public int Value;
    }

    public struct EnemiesToKillComponent : IComponentData
    {
        public int Value;
    }

    public struct EnemiesToKillIncrementComponent : IComponentData
    {
        public int Value;
    }

    public struct EnemySpawnerEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct EnemyTypesCountComponent : IComponentData
    {
        public int Value;
    }

    public struct HealthMultiplierComponent : IComponentData
    {
        public float Value;
    }
    
    public struct HealthValueForDeathComponent : IComponentData
    {
        public float Value;
    }

    public struct InitialBitmaskComponent : IComponentData
    {
        public uint Value;
    }

    public struct InputDashComponent : IComponentData
    {
        public uint Value;
    }

    public struct InputDeployComponent : IComponentData
    {
        public uint Value;
    }

    public struct InputDownComponent : IComponentData
    {
        public uint Value;
    }

    public struct InputEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct InputLeftComponent : IComponentData
    {
        public uint Value;
    }

    public struct InputNoneComponent : IComponentData
    {
        public uint Value; //Ignore this warning
    }

    public struct InputRightComponent : IComponentData
    {
        public uint Value;
    }

    public struct InputTurret1Component : IComponentData
    {
        public uint Value;
    }

    public struct InputTurret2Component : IComponentData
    {
        public uint Value;
    }

    public struct InputTurret3Component : IComponentData
    {
        public uint Value;
    }

    public struct InputUpComponent : IComponentData
    {
        public uint Value;
    }

    public struct LeftKeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct LevelComponent : IComponentData
    {
        public int Value;
    }
    
    public struct LevelToUnlockLineEnemyComponent : IComponentData
    {
        public int Value;
    }
    
    public struct LevelToUnlockSquareEnemyComponent : IComponentData
    {
        public int Value;
    }

    public struct LevelToUnlockTriangleEnemyComponent : IComponentData
    {
        public int Value;
    }

    public struct LineEnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct LineEnemyIndexComponent : IComponentData
    {
        public int Value;
    }

    public struct LootAmountComponent : IComponentData
    {
        public int Value;
    }

    public struct LootEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct LootMultiplierComponent : IComponentData
    {
        public float Value;
    }
    
    public struct LootPickupRadiusComponent : IComponentData
    {
        public float Value;
    }

    public struct MaxHealthComponent : IComponentData
    {
        public float Value;
    }
    
    public struct MinProjectileCountComponent : IComponentData
    {
        public int Value;
    }

    public struct MovementActiveComponent : IComponentData
    {
        public float Value;
    }

    public struct MovementNoneComponent : IComponentData
    {
        public float Value;
    }

    public struct MovementZigZagAmplitudeComponent : IComponentData
    {
        public float Value;
    }

    public struct MovementZigZagFrequencyComponent : IComponentData
    {
        public float Value;
    }

    public struct MoveSpeedComponent : IComponentData
    {
        public float Value;
    }

    public struct NoActionComponent : IComponentData
    {
        public int Value;
    }

    public struct PlayerEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct PlayerInputComponent : IComponentData
    {
        public uint Value;
    }

    // How many projectiles spawn per shot
    // Striker = 1, Scatter = 5
    public struct ProjectileCountComponent : IComponentData
    {
        public int Value;
    }

    public struct ProjectileLifetimeComponent : IComponentData
    {
        public float Value;
    }

    public struct RandomRangeStartComponent : IComponentData
    {
        public int Value;
    }

    public struct RandomSeedComponent : IComponentData
    {
        public Random Value;
    }

    public struct RangeComponent : IComponentData
    {
        public float Value;
    }

    public struct RightKeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct ScalingBaseComponent : IComponentData
    {
        public float Value;
    }

    public struct ScalingLevelOffsetComponent : IComponentData
    {
        public int Value;
    }

    public struct ScalingMinLevelComponent : IComponentData
    {
        public int Value;
    }
    
    public struct ScatterTurretUnlockLevelComponent : IComponentData
    {
        public int Value;
    }

    public struct SelectedTurretCostComponent : IComponentData
    {
        public int Value;
    }

    public struct SelectedTurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct SpawnRadiusComponent : IComponentData
    {
        public float Value;
    }

    public struct SpawnRateComponent : IComponentData
    {
        public float Value;
    }

    // The total angle of the spread in degrees
    // Striker = 0, Scatter = 30
    public struct SpreadComponent : IComponentData
    {
        public float Value;
    }
    
    public struct SpreadHalfMultiplierComponent : IComponentData
    {
        public float Value;
    }

    public struct SpreadZeroComponent : IComponentData
    {
        public float Value;
    }

    public struct SquareEnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct SquareEnemyIndexComponent : IComponentData
    {
        public int Value;
    }
    
    public struct TargetDefaultPositionComponent : IComponentData
    {
        public float Value;
    }

    public struct TargetPositionComponent : IComponentData
    {
        public float3 Value;
    }

    public struct TeamComponent : IComponentData
    {
        public int Value; // 0 = Player , 1 = Enemy and so on
    }

    public struct TimerComponent : IComponentData
    {
        public float Value;
    }
    
    public struct TimerExpiredComponent : IComponentData
    {
        public float Value;
    }

    public struct TriangleEnemyEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct TriangleEnemyIndexComponent : IComponentData
    {
        public int Value;
    }

    public struct Turret1KeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct Turret2KeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct Turret3KeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct TurretConfigEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct TurretCostComponent : IComponentData
    {
        public int Value;
    }

    public struct TurretEntityComponent : IComponentData
    {
        public Entity Entity;
    }

    public struct TurretLevelComponent : IComponentData
    {
        public int Value;
    }

    public struct UnlockedEnemiesComponent : IComponentData
    {
        public uint Value;
    }
    
    public struct UnlockedLineEnemyComponent : IComponentData
    {
        public uint Value;
    }
    
    public struct UnlockedNoneComponent : IComponentData
    {
        public uint Value;
    }
    
    public struct UnlockedSquareEnemyComponent : IComponentData
    {
        public uint Value;
    }

    public struct UnlockedTriangleEnemyComponent : IComponentData
    {
        public uint Value;
    }
    
    public struct UpgradeCostBaseMultiplierComponent : IComponentData
    {
        public float Value;
    }

    public struct UpgradeCostMultiplierComponent : IComponentData
    {
        public float Value;
    }

    public struct UpKeyComponent : IComponentData
    {
        public Key Value;
    }

    public struct VelocityComponent : IComponentData
    {
        public float2 Value;
    }

    // The starting number of enemies for Wave 1
    public struct WaveBaseEnemyCountComponent : IComponentData
    {
        public int Value;
    }

    // How many additional enemies are added each subsequent wave
    public struct WaveEnemyIncrementComponent : IComponentData
    {
        public int Value;
    }

    // Tracks the current wave number (1, 2, 3...)
    public struct WaveIndexComponent : IComponentData
    {
        public int Value;
    }

    // How long the preparation phase lasts in seconds (Reset Entity)
    public struct WavePrepDurationComponent : IComponentData
    {
        public float Value;
    }

    // 0 = Preparation Phase, 1 = Combat Phase
    public struct WaveStateComponent : IComponentData
    {
        public int Value;
    }
    
    public struct WaveStateCombatComponent : IComponentData
    {
        public int Value;
    }

    public struct WaveStatePrepComponent : IComponentData
    {
        public int Value;
    }

    // How many enemies are left to spawn in the current wave
    public struct WaveStockComponent : IComponentData
    {
        public int Value;
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