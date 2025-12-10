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

        private class EnemySpawnerBaker : Baker<EnemySpawnerEntity>
        {
            public override void Bake(EnemySpawnerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new EnemyPrefabComponent { EnemyPrefab = GetEntity(authoring.enemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new EnemySpawnRadiusComponent { EnemySpawnRadius = authoring.enemySpawnRadius });
                AddComponent(entity , new EnemySpawnRateComponent { EnemySpawnRate = authoring.enemySpawnRate });
                AddComponent(entity , new EnemySpawnTimerComponent { EnemySpawnTimer = authoring.enemySpawnRate });

                AddComponent(entity , new EnemySpawnerTag());

                uint validSeed = math.max(1 , authoring.randomSeed);
                AddComponent(entity , new RandomComponent { RandomValue = new Unity.Mathematics.Random(validSeed) });
            }
        }
    }
}