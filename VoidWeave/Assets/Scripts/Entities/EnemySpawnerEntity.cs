using Components;

namespace Entities
{
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

                AddComponent(entity , new EnemyEntityComponent { Entity = GetEntity(authoring.enemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new EnemySpawnRadiusComponent { Radius = authoring.enemySpawnRadius });
                AddComponent(entity , new EnemySpawnRateComponent { Rate = authoring.enemySpawnRate });
                AddComponent(entity , new EnemySpawnTimerComponent { Timer = authoring.enemySpawnRate });
                AddComponent(entity , new WaveIndexComponent { Index = 0 });
                AddComponent(entity , new WaveStateComponent { State = 0 });
                AddComponent(entity , new WaveStockComponent { Stock = 0 });
                AddComponent(entity , new WaveTimerComponent { Timer = authoring.waveTimer });

                AddComponent(entity , new EnemySpawnerTag());

                uint validSeed = math.max(1 , authoring.randomSeed);
                AddComponent(entity , new RandomComponent { Random = new Unity.Mathematics.Random(validSeed) });
            }
        }
    }
}