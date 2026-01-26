using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities
{
    public class GameConfigEntity : MonoBehaviour
    {
        [SerializeField] private float damageMultiplierPerLevel;
        [SerializeField] private int enemiesToKill;
        [SerializeField] private int enemiesToKillIncrement;
        [SerializeField] private GameObject enemySpawnerPrefab;
        [SerializeField] private float healthMultiplierPerLevel;
        [SerializeField] private float lootMultiplierPerLevel;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private int startingLevel;
        [SerializeField] private int startingResources;
        [SerializeField] private GameObject turretConfigPrefab;

        private class GameConfigBaker : Baker<GameConfigEntity>
        {
            public override void Bake(GameConfigEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                int enemiesToKill = math.max(1 , authoring.enemiesToKill);
                int startingLevel = math.max(1 , authoring.startingLevel);
                uint initialMask = 1;
                initialMask |= (uint)math.select(0 , 2 , startingLevel >= 2);
                initialMask |= (uint)math.select(0 , 4 , startingLevel >= 3);

                AddComponent(entity , new CurrentEnergyComponent { Energy = authoring.startingResources });
                AddComponent(entity , new DamageMultiplierComponent { DamageMultiplier = authoring.damageMultiplierPerLevel });
                AddComponent(entity , new EnemiesToKillComponent { EnemiesToKill = enemiesToKill });
                AddComponent(entity , new EnemiesKilledComponent());
                AddComponent(entity , new EnemiesToKillIncrementComponent { EnemiesToKillIncrement = authoring.enemiesToKillIncrement });
                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(authoring.enemySpawnerPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new HealthMultiplierComponent { HealthMultiplier = authoring.healthMultiplierPerLevel });
                AddComponent(entity , new LevelComponent { Level = authoring.startingLevel });
                AddComponent(entity , new LootMultiplierComponent { LootMultiplier = authoring.lootMultiplierPerLevel });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(authoring.playerPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(authoring.turretConfigPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new UnlockedEnemiesComponent { UnlockedEnemiesBitmask = initialMask });
                
                AddComponent(entity , new InitializeGameTag());
            }
        }
    }
}