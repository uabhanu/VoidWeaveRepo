namespace Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

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
                
                uint seed = math.max(1 , authoring.randomSeed);
                
                AddComponent(entity , new ActiveWaveStateComponent { ActiveWaveState = authoring.activeWaveState });
                AddComponent(entity , new EnemySpawnerTag());
                AddComponent(entity , new EnemyTypesCountComponent { EnemyTypesCount = authoring.enemyTypesCount });
                AddComponent(entity , new InitialBitmaskComponent { InitialBitmask = authoring.initialBitmask });
                AddComponent(entity , new LineEnemyEntityComponent { Entity = GetEntity(authoring.lineEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LineEnemyIndexComponent { LineEnemyIndex = authoring.lineEnemyIndex });
                AddComponent(entity , new RandomRangeStartComponent { RandomRangeStartValue = authoring.randomRangeStartValue });
                AddComponent(entity , new RandomSeedComponent { RandomSeed = new Unity.Mathematics.Random(seed) });
                AddComponent(entity , new SpawnRadiusComponent { Radius = authoring.enemySpawnRadius });
                AddComponent(entity , new SpawnRateComponent { Rate = authoring.enemySpawnRate });
                AddComponent(entity , new SquareEnemyEntityComponent { Entity = GetEntity(authoring.squareEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new SquareEnemyIndexComponent { SquareEnemyIndex = authoring.squareEnemyIndex });
                AddComponent(entity , new TimerComponent { Timer = authoring.wavePrepDuration });
                AddComponent(entity , new TriangleEnemyEntityComponent { Entity = GetEntity(authoring.triangleEnemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TriangleEnemyIndexComponent { TriangleEnemyIndex = authoring.triangleEnemyIndex });
                AddComponent(entity , new WaveBaseEnemyCountComponent { Count = authoring.baseEnemies });
                AddComponent(entity , new WaveEnemyIncrementComponent { Count = authoring.enemyIncrement });
                AddComponent(entity , new WaveIndexComponent());
                AddComponent(entity , new WavePrepDurationComponent { Duration = authoring.wavePrepDuration });
                AddComponent(entity , new WaveStateComponent());
                AddComponent(entity , new WaveStockComponent());
            }
        }
    }
}