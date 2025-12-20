namespace Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class EnemySpawnerEntity : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float enemySpawnRadius;
        [SerializeField] private float enemySpawnRate;
        [SerializeField] private uint randomSeed;
        [SerializeField] private int baseEnemies;
        [SerializeField] private int enemyIncrement;
        [SerializeField] private float wavePrepDuration;

        private class EnemySpawnerBaker : Baker<EnemySpawnerEntity>
        {
            public override void Bake(EnemySpawnerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // --- SPAWNING ---
                // NOTE: This might look redundant, but it is the ECS "State vs Config" pattern.
                // RATE (Config): Stores the "Reset Value" (e.g., 2.1s). This NEVER changes.
                // TIMER (State): Stores the "Countdown". It changes every frame (2.1 -> 2.0 -> ... -> 0).
                // When Timer hits 0, we need the Rate component to know what to reset it back to.
                AddComponent(entity , new EnemyEntityComponent { Entity = GetEntity(authoring.enemyPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new EnemySpawnRadiusComponent { Radius = authoring.enemySpawnRadius });
                AddComponent(entity , new EnemySpawnRateComponent { Rate = authoring.enemySpawnRate });
                AddComponent(entity , new EnemySpawnTimerComponent { Timer = authoring.enemySpawnRate });
                AddComponent(entity , new WaveBaseEnemyCountComponent { Count = authoring.baseEnemies });
                AddComponent(entity , new WaveEnemyIncrementComponent { Count = authoring.enemyIncrement });
                AddComponent(entity , new WaveIndexComponent { Index = 0 });
                
                // DURATION (Config): The "Reset Value" for breaks between waves (e.g., 30s).
                // TIMER (State): The live countdown for the current phase. 
                // We initialize Timer with Duration so the very first Prep phase lasts the correct amount of time.
                AddComponent(entity , new WavePrepDurationComponent { Duration = authoring.wavePrepDuration });
                AddComponent(entity , new WaveStateComponent { State = 0 });
                AddComponent(entity , new WaveStockComponent { Stock = 0 });
                AddComponent(entity , new WaveTimerComponent { Timer = authoring.wavePrepDuration });

                AddComponent(entity , new EnemySpawnerTag());

                uint validSeed = math.max(1 , authoring.randomSeed);
                AddComponent(entity , new RandomComponent { Random = new Unity.Mathematics.Random(validSeed) });
            }
        }
    }
}