using Components;
using Unity.Entities;
using UnityEngine;

namespace Entities
{
    public class GameConfigEntity : MonoBehaviour
    {
        [SerializeField] private GameObject enemySpawnerPrefab;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject turretConfigPrefab;

        private class GameConfigBaker : Baker<GameConfigEntity>
        {
            public override void Bake(GameConfigEntity gameConfigEntity)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(gameConfigEntity.enemySpawnerPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(gameConfigEntity.playerPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(gameConfigEntity.turretConfigPrefab , TransformUsageFlags.Dynamic) });
                
                AddComponent(entity , new InitializeGameTag());
            }
        }
    }
}