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
        [SerializeField] private float lootMultiplierPerLevel;
        [SerializeField] private float movementActiveValue;
        [SerializeField] private float movementNoneValue;
        [SerializeField] private GameObject playerEntityPrefab;
        [SerializeField] private float scalingBaseValue;
        [SerializeField] private int scalingLevelOffset;
        [SerializeField] private int scalingMinLevel;
        [SerializeField] private int startingLevel;
        [SerializeField] private int startingResources;
        [SerializeField] private GameObject turretConfigEntityPrefab;

        private class GameConfigBaker : Baker<GameManagerEntity>
        {
            public override void Bake(GameManagerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                int enemiesToKill = math.max(1 , authoring.enemiesToKill);
                int startingLevel = math.max(1 , authoring.startingLevel);
                uint initialMask = 1;
                initialMask |= (uint)math.select(0 , 2 , startingLevel >= 2);
                initialMask |= (uint)math.select(0 , 4 , startingLevel >= 3);

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