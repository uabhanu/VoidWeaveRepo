using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities
{
    public class GameManagerEntity : MonoBehaviour
    {
        #region Variables

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
        [SerializeField] private float spreadHalfMultiplier;
        [SerializeField] private float spreadZero;
        [SerializeField] private int squareEnemyUnlockLevel; //The When
        [SerializeField] private int startingLevel;
        [SerializeField] private int startingResources;
        [SerializeField] private float timerExpired;
        [SerializeField] private int triangleEnemyUnlockLevel; //The When
        [SerializeField] private GameObject turretConfigEntityPrefab;
        [SerializeField] private uint unlockedLineEnemy; //The What
        [SerializeField] private uint unlockedNone;
        [SerializeField] private uint unlockedSquareEnemy; //The What
        [SerializeField] private uint unlockedTriangleEnemy; //The What

        #endregion

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

                AddComponent(entity, new BulletRotationOffsetComponent { Offset = authoring.bulletRotationOffset });
                AddComponent(entity , new BoundaryOffsetComponent { Offset = authoring.boundaryOffset });
                AddComponent(entity , new CameraOrthographicSizeComponent { Size = authoring.cameraOrthographicSize });
                AddComponent(entity , new CollisionActiveComponent { CollisionActive = authoring.collisionActive });
                AddComponent(entity , new CollisionNoneComponent { CollisionNone = authoring.collisionNone });
                AddComponent(entity , new CurrentEnergyComponent { Energy = authoring.startingResources });
                AddComponent(entity , new DamageMultiplierComponent { DamageMultiplier = authoring.damageMultiplierPerLevel });
                AddComponent(entity , new DashCooldownDefaultComponent { DashCooldownDefault = authoring.dashCooldownDefault });
                AddComponent(entity , new DashDurationDefaultComponent { DashDurationDefault = authoring.dashDurationDefault });
                AddComponent(entity , new DoActionComponent { DoAction = authoring.doAction });
                AddComponent(entity , new EnemiesToKillComponent { EnemiesToKill = enemiesToKill });
                AddComponent(entity , new EnemiesKilledComponent());
                AddComponent(entity , new EnemiesToKillIncrementComponent { EnemiesToKillIncrement = authoring.enemiesToKillIncrement });
                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(authoring.enemySpawnerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new HealthMultiplierComponent { HealthMultiplier = authoring.healthMultiplierPerLevel });
                AddComponent(entity , new HealthValueForDeathComponent { HealthValueForDeath = authoring.healthValueForDeath });
                AddComponent(entity , new InputEntityComponent { Entity = GetEntity(authoring.inputEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LevelComponent { Level = authoring.startingLevel });
                AddComponent(entity , new LevelToUnlockLineEnemyComponent { LevelToUnlockLineEnemy = authoring.lineEnemyUnlockLevel });
                AddComponent(entity , new LevelToUnlockSquareEnemyComponent { LevelToUnlockSquareEnemy = authoring.squareEnemyUnlockLevel });
                AddComponent(entity , new LevelToUnlockTriangleEnemyComponent { LevelToUnlockTriangleEnemy = authoring.triangleEnemyUnlockLevel });
                AddComponent(entity , new LootMultiplierComponent { LootMultiplier = authoring.lootMultiplierPerLevel });
                AddComponent(entity , new LootPickupRadiusComponent { Radius = authoring.lootPickupRadius });
                AddComponent(entity, new MinProjectileCountComponent { Count = authoring.minProjectileCount });
                AddComponent(entity , new MovementActiveComponent { MovementActive = authoring.movementActive });
                AddComponent(entity , new MovementNoneComponent { MovementNone = authoring.movementNone });
                AddComponent(entity , new NoActionComponent { NoActionValue = authoring.noAction });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(authoring.playerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new ScalingBaseComponent { ScalingBase = authoring.scalingBase });
                AddComponent(entity , new ScalingLevelOffsetComponent { ScalingLevelOffset = authoring.scalingLevelOffset });
                AddComponent(entity , new ScalingMinLevelComponent { ScalingMinLevel = authoring.scalingMinLevel });
                AddComponent(entity, new SpreadHalfMultiplierComponent { HalfMultiplier = authoring.spreadHalfMultiplier });
                AddComponent(entity, new SpreadZeroComponent { Zero = authoring.spreadZero });
                AddComponent(entity , new TimerExpiredComponent { TimerExpired = authoring.timerExpired });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(authoring.turretConfigEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new UnlockedEnemiesComponent { UnlockedEnemiesBitmask = initialMask });
                AddComponent(entity , new UnlockedLineEnemyComponent { UnlockedLineEnemy = authoring.unlockedLineEnemy });
                AddComponent(entity , new UnlockedNoneComponent { UnlockedNone = authoring.unlockedNone });
                AddComponent(entity , new UnlockedSquareEnemyComponent { UnlockedSquareEnemy = authoring.unlockedSquareEnemy });
                AddComponent(entity , new UnlockedTriangleEnemyComponent { UnlockedTriangleEnemy = authoring.unlockedTriangleEnemy });

                AddComponent(entity , new InitializeGameTag());
            }
        }

        #endregion
    }
}