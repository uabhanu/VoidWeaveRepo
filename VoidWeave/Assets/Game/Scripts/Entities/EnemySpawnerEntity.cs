namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using Random = Unity.Mathematics.Random;

    public class EnemySpawnerEntity : MonoBehaviour
    {
        [SerializeField] private int activeWaveState;
        [SerializeField] private int baseEnemies;
        [SerializeField] private int enemyIncrement;
        [SerializeField] private float enemySpawnRadius;
        [SerializeField] private float enemySpawnRate;
        [SerializeField] private int enemyTypesCount;
        [SerializeField] private uint initialBitmask;
        [SerializeField] private int lineEnemyIndex;
        [SerializeField] private GameObject lineEnemyPrefab;
        [SerializeField] private int randomRangeStartValue;
        [SerializeField] private uint randomSeed;
        [SerializeField] private uint randomSeedMin;
        [SerializeField] private int squareEnemyIndex;
        [SerializeField] private GameObject squareEnemyPrefab;
        [SerializeField] private int triangleEnemyIndex;
        [SerializeField] private GameObject triangleEnemyPrefab;
        [SerializeField] private float wavePrepDuration;

        private class EnemySpawnerBaker : Baker<EnemySpawnerEntity>
        {
            public override void Bake(EnemySpawnerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Random Seed Entity and Random Seed Entity Min both are set as 1 in the inspector but still this is correct:
                // This logic acts as a mandatory safety clamp. It guarantees the seed is never 0 by forcing it to be at least the minimum.
                // Even if both inputs are 1, the operation correctly resolves to 1, ensuring the RNG always initializes with a valid, non-zero value.
                uint seed = math.max(authoring.randomSeedMin , authoring.randomSeed);

                AddComponent(entity , new ActiveWaveStateComponent { Value = authoring.activeWaveState });
                AddComponent(entity , new EnemySpawnerTag());
                AddComponent(entity , new EnemyTypesCountComponent { Value = authoring.enemyTypesCount });
                AddComponent(entity , new InitialBitmaskComponent { Value = authoring.initialBitmask });
                AddComponent(entity , new LineEnemyEntityComponent { Entity = GetEntity(authoring.lineEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LineEnemyIndexComponent { Value = authoring.lineEnemyIndex });
                AddComponent(entity , new RandomRangeStartComponent { Value = authoring.randomRangeStartValue });
                AddComponent(entity , new RandomSeedComponent { Value = new Random(seed) });
                AddComponent(entity , new SpawnRadiusComponent { Value = authoring.enemySpawnRadius });
                AddComponent(entity , new SpawnRateComponent { Value = authoring.enemySpawnRate });
                AddComponent(entity , new SquareEnemyEntityComponent { Entity = GetEntity(authoring.squareEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new SquareEnemyIndexComponent { Value = authoring.squareEnemyIndex });
                AddComponent(entity , new TimerComponent { Value = authoring.wavePrepDuration });
                AddComponent(entity , new TriangleEnemyEntityComponent { Entity = GetEntity(authoring.triangleEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TriangleEnemyIndexComponent { Value = authoring.triangleEnemyIndex });
                AddComponent(entity , new WaveBaseEnemyCountComponent { Value = authoring.baseEnemies });
                AddComponent(entity , new WaveEnemyIncrementComponent { Value = authoring.enemyIncrement });
                AddComponent(entity , new WaveIndexComponent());
                AddComponent(entity , new WavePrepDurationComponent { Value = authoring.wavePrepDuration });
                AddComponent(entity , new WaveStateComponent());
                AddComponent(entity , new WaveStockComponent());
            }
        }
    }
}