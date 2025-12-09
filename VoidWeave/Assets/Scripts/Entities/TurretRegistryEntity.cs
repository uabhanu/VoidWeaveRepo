namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class TurretRegistryEntity : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject turretPrefab;

        class TurretRegistryBaker : Baker<TurretRegistryEntity>
        {
            public override void Bake(TurretRegistryEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new BulletPrefabComponent { BulletPrefab = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TurretPrefabComponent { TurretPrefab = GetEntity(authoring.turretPrefab , TransformUsageFlags.Dynamic) });
            }
        }
    }
}