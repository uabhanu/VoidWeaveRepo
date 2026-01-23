namespace Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class EnemySpawnerEntity : MonoBehaviour
    {
        [SerializeField] private int baseEnemies;
        [SerializeField] private float damageMultiplierPerLevel;
        [SerializeField] private int enemiesToKill;
        [SerializeField] private int enemiesToKillIncrement;
        [SerializeField] private int enemyIncrement;
        [SerializeField] private float enemySpawnRadius;
        [SerializeField] private float enemySpawnRate;
        [SerializeField] private float healthMultiplierPerLevel;
        [SerializeField] private GameObject lineEnemyPrefab;
        [SerializeField] private float lootMultiplierPerLevel;
        [SerializeField] private uint randomSeed;
        [SerializeField] private GameObject squareEnemyPrefab;
        [SerializeField] private int startingLevel;
        [SerializeField] private GameObject triangleEnemyPrefab;
        [SerializeField] private float wavePrepDuration;

        private class EnemySpawnerBaker : Baker<EnemySpawnerEntity>
        {
            public override void Bake(EnemySpawnerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                int enemiesToKill = math.max(1 , authoring.enemiesToKill);
                uint seed = math.max(1 , authoring.randomSeed);
                int startingLevel = math.max(1 , authoring.startingLevel);

                uint initialMask = 1;
                
                initialMask |= (uint)math.select(0 , 2 , startingLevel >= 2);
                initialMask |= (uint)math.select(0 , 4 , startingLevel >= 3);

                AddComponent(entity , new DamageMultiplierComponent { DamageMultiplier = authoring.damageMultiplierPerLevel });
                AddComponent(entity , new EnemiesKilledComponent());
                AddComponent(entity , new EnemiesToKillComponent { EnemiesToKill = enemiesToKill });
                AddComponent(entity , new EnemiesToKillIncrementComponent { EnemiesToKillIncrement = authoring.enemiesToKillIncrement });
                AddComponent(entity , new EnemySpawnerTag());
                AddComponent(entity , new HealthMultiplierComponent { HealthMultiplier = authoring.healthMultiplierPerLevel });
                AddComponent(entity , new LevelComponent { Level = startingLevel });
                AddComponent(entity , new LineEnemyEntityComponent { Entity = GetEntity(authoring.lineEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LootMultiplierComponent { LootMultiplier = authoring.lootMultiplierPerLevel });
                AddComponent(entity , new RandomComponent { Random = new Unity.Mathematics.Random(seed) });
                AddComponent(entity , new SpawnRadiusComponent { Radius = authoring.enemySpawnRadius });
                AddComponent(entity , new SpawnRateComponent { Rate = authoring.enemySpawnRate });
                AddComponent(entity , new SquareEnemyEntityComponent { Entity = GetEntity(authoring.squareEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TimerComponent { Timer = authoring.wavePrepDuration });
                AddComponent(entity , new TriangleEnemyEntityComponent { Entity = GetEntity(authoring.triangleEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new UnlockedEnemiesComponent { UnlockedEnemiesBitmask = initialMask });
                AddComponent(entity , new WaveBaseEnemyCountComponent { Count = authoring.baseEnemies });
                AddComponent(entity , new WaveEnemyIncrementComponent { Count = authoring.enemyIncrement });
                AddComponent(entity , new WaveIndexComponent { Index = 0 });
                AddComponent(entity , new WavePrepDurationComponent { Duration = authoring.wavePrepDuration });
                AddComponent(entity , new WaveStateComponent { State = 0 });
                AddComponent(entity , new WaveStockComponent { Stock = 0 });
            }
        }
    }
}