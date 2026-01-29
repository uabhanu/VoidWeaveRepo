using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities
{
    public class GameManagerEntity : MonoBehaviour
    {
        [SerializeField] private int collisionActiveValue;
        [SerializeField] private int collisionNoneValue;
        [SerializeField] private float damageMultiplierPerLevel;
        [SerializeField] private int enemiesToKill;
        [SerializeField] private int enemiesToKillIncrement;
        [SerializeField] private GameObject enemySpawnerEntityPrefab;
        [SerializeField] private float healthMultiplierPerLevel;
        [SerializeField] private GameObject inputEntityPrefab;
        [SerializeField] private int lineEnemyUnlockLevel; //The When
        [SerializeField] private float lootMultiplierPerLevel;
        [SerializeField] private int minEnemiesToKill;
        [SerializeField] private int minStartingLevel;
        [SerializeField] private float movementActiveValue;
        [SerializeField] private float movementNoneValue;
        [SerializeField] private GameObject playerEntityPrefab;
        [SerializeField] private float scalingBaseValue;
        [SerializeField] private int scalingLevelOffset;
        [SerializeField] private int scalingMinLevel;
        [SerializeField] private int squareEnemyUnlockLevel; //The When
        [SerializeField] private int startingLevel;
        [SerializeField] private int startingResources;
        [SerializeField] private int triangleEnemyUnlockLevel; //The When
        [SerializeField] private GameObject turretConfigEntityPrefab;
        [SerializeField] private uint unlockedLineEnemyValue; //The What
        [SerializeField] private uint unlockedNoneValue;
        [SerializeField] private uint unlockedSquareEnemyValue; //The What
        [SerializeField] private uint unlockedTriangleEnemyValue; //The What

        private class GameConfigBaker : Baker<GameManagerEntity>
        {
            public override void Bake(GameManagerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                int enemiesToKill = math.max(authoring.minEnemiesToKill , authoring.enemiesToKill);
                int startingLevel = math.max(authoring.minStartingLevel , authoring.startingLevel);
                
                uint initialMask = authoring.unlockedNoneValue;
                initialMask |= math.select(authoring.unlockedNoneValue , authoring.unlockedLineEnemyValue , startingLevel >= authoring.lineEnemyUnlockLevel);
                initialMask |= math.select(authoring.unlockedNoneValue , authoring.unlockedTriangleEnemyValue , startingLevel >= authoring.triangleEnemyUnlockLevel);
                initialMask |= (uint)math.select(authoring.unlockedNoneValue , authoring.unlockedSquareEnemyValue , startingLevel >= authoring.squareEnemyUnlockLevel);

                AddComponent(entity , new CollisionActiveValueComponent { CollisionActiveValue = authoring.collisionActiveValue });
                AddComponent(entity , new CollisionNoneValueComponent { CollisionNoneValue = authoring.collisionNoneValue });
                AddComponent(entity , new CurrentEnergyComponent { Energy = authoring.startingResources });
                AddComponent(entity , new DamageMultiplierComponent { DamageMultiplier = authoring.damageMultiplierPerLevel });
                AddComponent(entity , new EnemiesToKillComponent { EnemiesToKill = enemiesToKill });
                AddComponent(entity , new EnemiesKilledComponent());
                AddComponent(entity , new EnemiesToKillIncrementComponent { EnemiesToKillIncrement = authoring.enemiesToKillIncrement });
                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(authoring.enemySpawnerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new HealthMultiplierComponent { HealthMultiplier = authoring.healthMultiplierPerLevel });
                AddComponent(entity , new InputEntityComponent { Entity = GetEntity(authoring.inputEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LevelComponent { Level = authoring.startingLevel });
                AddComponent(entity , new LootMultiplierComponent { LootMultiplier = authoring.lootMultiplierPerLevel });
                AddComponent(entity , new MovementActiveValueComponent { MovementActiveValue = authoring.movementActiveValue });
                AddComponent(entity , new MovementNoneValueComponent { MovementNoneValue = authoring.movementNoneValue });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(authoring.playerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new ScalingBaseValueComponent { ScalingBaseValue = authoring.scalingBaseValue });
                AddComponent(entity , new ScalingLevelOffsetValueComponent { ScalingLevelOffsetValue = authoring.scalingLevelOffset });
                AddComponent(entity , new ScalingMinLevelValueComponent { ScalingMinLevelValue = authoring.scalingMinLevel });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(authoring.turretConfigEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new UnlockedEnemiesComponent { UnlockedEnemiesBitmask = initialMask });
                
                AddComponent(entity , new InitializeGameTag());
            }
        }
    }
}