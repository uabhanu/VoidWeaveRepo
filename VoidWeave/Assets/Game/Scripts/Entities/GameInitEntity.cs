namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class GameInitEntity : MonoBehaviour
    {
        [SerializeField] private GameObject enemySpawnerEntityPrefab;
        [SerializeField] private GameObject gameBackgroundEntityPrefab;
        [SerializeField] private GameObject inputEntityPrefab;
        [SerializeField] private GameObject playerEntityPrefab;
        [SerializeField] private GameObject turretConfigEntityPrefab;

        private class GameInitEntityBaker : Baker<GameInitEntity>
        {
            public override void Bake(GameInitEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(authoring.enemySpawnerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new GameBackgroundEntityComponent { Entity = GetEntity(authoring.gameBackgroundEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new InputEntityComponent { Entity = GetEntity(authoring.inputEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(authoring.playerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(authoring.turretConfigEntityPrefab , TransformUsageFlags.Dynamic) });
            }
        }
    }
}