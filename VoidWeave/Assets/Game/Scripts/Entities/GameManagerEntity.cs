namespace Game.Scripts.Entities
{
    using Game.Scripts.Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class GameManagerEntity : MonoBehaviour
    {
        #region Baker

        private class GameManagerBaker : Baker<GameManagerEntity>
        {
            public override void Bake(GameManagerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                int enemiesToKill = math.max(authoring.minEnemiesToKill , authoring.enemiesToKill);
                int startingLevel = math.max(authoring.minStartingLevel , authoring.startingLevel);

                uint initialMask = authoring.unlockedNone;
                initialMask |= math.select(authoring.unlockedNone , authoring.unlockedLineEnemy , startingLevel >= authoring.lineEnemyUnlockLevel);
                initialMask |= math.select(authoring.unlockedNone , authoring.unlockedTriangleEnemy , startingLevel >= authoring.triangleEnemyUnlockLevel);
                initialMask |= math.select(authoring.unlockedNone , authoring.unlockedSquareEnemy , startingLevel >= authoring.squareEnemyUnlockLevel);

                AddComponent(entity , new BeamTurretUnlockLevelComponent { Value = authoring.beamTurretUnlockLevel });
                AddComponent(entity , new BulletRotationOffsetComponent { Value = authoring.bulletRotationOffset });
                AddComponent(entity , new BoundaryOffsetComponent { Value = authoring.boundaryOffset });
                AddComponent(entity , new CameraOrthographicSizeComponent { Value = authoring.cameraOrthographicSize });
                AddComponent(entity , new CollisionActiveComponent { Value = authoring.collisionActive });
                AddComponent(entity , new CollisionNoneComponent { Value = authoring.collisionNone });
                AddComponent(entity , new CurrentEnergyComponent { Value = authoring.startingResources });
                AddComponent(entity , new DamageMultiplierComponent { Value = authoring.damageMultiplierPerLevel });
                AddComponent(entity , new DashCooldownDefaultComponent { Value = authoring.dashCooldownDefault });
                AddComponent(entity , new DashDurationDefaultComponent { Value = authoring.dashDurationDefault });
                AddComponent(entity , new DoActionComponent { Value = authoring.doAction });
                AddComponent(entity , new EnemiesToKillComponent { Value = enemiesToKill });
                AddComponent(entity , new EnemiesKilledComponent());
                AddComponent(entity , new EnemiesToKillIncrementComponent { Value = authoring.enemiesToKillIncrement });
                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(authoring.enemySpawnerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new HealthMultiplierComponent { Value = authoring.healthMultiplierPerLevel });
                AddComponent(entity , new HealthValueForDeathComponent { Value = authoring.healthValueForDeath });
                AddComponent(entity , new InputEntityComponent { Entity = GetEntity(authoring.inputEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LevelComponent { Value = authoring.startingLevel });
                AddComponent(entity , new LevelToUnlockLineEnemyComponent { Value = authoring.lineEnemyUnlockLevel });
                AddComponent(entity , new LevelToUnlockSquareEnemyComponent { Value = authoring.squareEnemyUnlockLevel });
                AddComponent(entity , new LevelToUnlockTriangleEnemyComponent { Value = authoring.triangleEnemyUnlockLevel });
                AddComponent(entity , new LootMultiplierComponent { Value = authoring.lootMultiplierPerLevel });
                AddComponent(entity , new LootPickupRadiusComponent { Value = authoring.lootPickupRadius });
                AddComponent(entity , new MinProjectileCountComponent { Value = authoring.minProjectileCount });
                AddComponent(entity , new MovementActiveComponent { Value = authoring.movementActive });
                AddComponent(entity , new MovementNoneComponent { Value = authoring.movementNone });
                AddComponent(entity , new NoActionComponent { Value = authoring.noAction });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(authoring.playerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new ScalingBaseComponent { Value = authoring.scalingBase });
                AddComponent(entity , new ScalingLevelOffsetComponent { Value = authoring.scalingLevelOffset });
                AddComponent(entity , new ScalingMinLevelComponent { Value = authoring.scalingMinLevel });
                AddComponent(entity , new ScatterTurretUnlockLevelComponent { Value = authoring.scatterTurretUnlockLevel });
                AddComponent(entity , new SpreadHalfMultiplierComponent { Value = authoring.spreadHalfMultiplier });
                AddComponent(entity , new SpreadZeroComponent { Value = authoring.spreadZero });
                AddComponent(entity , new TargetDefaultPositionComponent { Value = authoring.targetDefaultPosition });
                AddComponent(entity , new TimerExpiredComponent { Value = authoring.timerExpired });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(authoring.turretConfigEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new UnlockedEnemiesComponent { Value = initialMask });
                AddComponent(entity , new UnlockedLineEnemyComponent { Value = authoring.unlockedLineEnemy });
                AddComponent(entity , new UnlockedNoneComponent { Value = authoring.unlockedNone });
                AddComponent(entity , new UnlockedSquareEnemyComponent { Value = authoring.unlockedSquareEnemy });
                AddComponent(entity , new UnlockedTriangleEnemyComponent { Value = authoring.unlockedTriangleEnemy });
                AddComponent(entity , new UpgradeCostBaseMultiplierComponent { Value = authoring.upgradeCostBaseMultiplier });
                AddComponent(entity , new UpgradeCostMultiplierComponent { Value = authoring.upgradeCostMultiplier });
                AddComponent(entity , new WaveStateCombatComponent { Value = authoring.waveStateCombat });
                AddComponent(entity , new WaveStatePrepComponent { Value = authoring.waveStatePrep });

                AddComponent(entity , new InitializeGameTag());
            }
        }

        #endregion
        #region Variables

        [SerializeField] private int beamTurretUnlockLevel;
        [SerializeField] private float boundaryOffset;
        [SerializeField] private float bulletRotationOffset; // PI / 2
        [SerializeField] private float cameraOrthographicSize;
        [SerializeField] private int collisionActive;
        [SerializeField] private int collisionNone;
        [SerializeField] private float damageMultiplierPerLevel;
        [SerializeField] private float dashCooldownDefault;
        [SerializeField] private float dashDurationDefault;
        [SerializeField] private int doAction;
        [SerializeField] private int noAction;
        [SerializeField] private int enemiesToKill;
        [SerializeField] private int enemiesToKillIncrement;
        [SerializeField] private GameObject enemySpawnerEntityPrefab;
        [SerializeField] private float healthMultiplierPerLevel;
        [SerializeField] private float healthValueForDeath;
        [SerializeField] private GameObject inputEntityPrefab;
        [SerializeField] private int lineEnemyUnlockLevel; //The When
        [SerializeField] private float lootMultiplierPerLevel;
        [SerializeField] private float lootPickupRadius;
        [SerializeField] private int minEnemiesToKill;
        [SerializeField] private int minProjectileCount = 1;
        [SerializeField] private int minStartingLevel;
        [SerializeField] private float movementActive;
        [SerializeField] private float movementNone;
        [SerializeField] private GameObject playerEntityPrefab;
        [SerializeField] private float scalingBase;
        [SerializeField] private int scalingLevelOffset;
        [SerializeField] private int scalingMinLevel;
        [SerializeField] private int scatterTurretUnlockLevel;
        [SerializeField] private float spreadHalfMultiplier;
        [SerializeField] private float spreadZero;
        [SerializeField] private int squareEnemyUnlockLevel; //The When
        [SerializeField] private int startingLevel;
        [SerializeField] private int startingResources;
        [Tooltip("Chosen this high value on purpose to make turret shoot nothing when there is no target")] [SerializeField] private float targetDefaultPosition;
        [SerializeField] private float timerExpired;
        [SerializeField] private int triangleEnemyUnlockLevel; //The When
        [SerializeField] private GameObject turretConfigEntityPrefab;
        [SerializeField] private uint unlockedLineEnemy; //The What
        [SerializeField] private uint unlockedNone;
        [SerializeField] private uint unlockedSquareEnemy; //The What
        [SerializeField] private uint unlockedTriangleEnemy; //The What
        [SerializeField] private float upgradeCostBaseMultiplier;
        [SerializeField] private float upgradeCostMultiplier;
        [SerializeField] private int waveStateCombat;
        [SerializeField] private int waveStatePrep;

        #endregion
    }
}