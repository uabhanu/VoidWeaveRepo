namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class EnemySpawnerEntity : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float enemySpawnRadius;
        [SerializeField] private float enemySpawnRate;
        [SerializeField] private uint randomSeed;
        [SerializeField] private float waveTimer;

        private class EnemySpawnerBaker : Baker<EnemySpawnerEntity>
        {
            public override void Bake(EnemySpawnerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new EnemyEntityComponent { EnemyEntity = GetEntity(authoring.enemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new EnemySpawnRadiusComponent { EnemySpawnRadius = authoring.enemySpawnRadius });
                AddComponent(entity , new EnemySpawnRateComponent { EnemySpawnRate = authoring.enemySpawnRate });
                AddComponent(entity , new EnemySpawnTimerComponent { EnemySpawnTimer = authoring.enemySpawnRate });
                AddComponent(entity , new WaveIndexComponent { WaveIndex = 0 });
                AddComponent(entity , new WaveStateComponent { WaveState = 0 });
                AddComponent(entity , new WaveStockComponent { WaveStock = 0 });
                AddComponent(entity , new WaveTimerComponent { WaveTimer = authoring.waveTimer });

                AddComponent(entity , new EnemySpawnerTag());

                uint validSeed = math.max(1 , authoring.randomSeed);
                AddComponent(entity , new RandomComponent { RandomValue = new Unity.Mathematics.Random(validSeed) });
            }
        }
    }
}