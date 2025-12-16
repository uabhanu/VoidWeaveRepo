namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class ScatterTurretEntity : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cooldownTime;
        [SerializeField] private float damage;
        [SerializeField] private int deploymentCost;
        [SerializeField] private float fireRate;
        [SerializeField] private int projectileCount;
        [SerializeField] private float range;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;

        private class ScatterTurretBaker : Baker<ScatterTurretEntity>
        {
            public override void Bake(ScatterTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new ScatterTurretCostComponent { Cost = authoring.deploymentCost });
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                AddComponent(entity , new TurretCooldownComponent { Timer = authoring.cooldownTime });
                AddComponent(entity , new TurretDamageComponent { Damage = authoring.damage });
                AddComponent(entity , new TurretFireRateComponent { Rate = authoring.fireRate });
                AddComponent(entity , new TurretProjectileCountComponent { Count = authoring.projectileCount });
                AddComponent(entity , new TurretRangeComponent { Range = authoring.range });
                AddComponent(entity , new TurretSpreadComponent { Degrees = authoring.spreadDegrees });
                
                AddComponent(entity , new ScatterTurretTag());
            }
        }
    }
}