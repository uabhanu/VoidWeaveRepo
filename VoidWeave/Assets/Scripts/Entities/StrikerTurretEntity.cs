namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class StrikerTurretEntity : MonoBehaviour
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

        class StrikerBaker : Baker<StrikerTurretEntity>
        {
            public override void Bake(StrikerTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new CooldownComponent { Timer = authoring.cooldownTime });
                AddComponent(entity , new DamageComponent { Damage = authoring.damage });
                AddComponent(entity , new FireRateComponent { FireRate = authoring.fireRate });
                AddComponent(entity, new ProjectileCountComponent { Count = authoring.projectileCount });
                AddComponent(entity , new RangeComponent { Range = authoring.range });
                AddComponent(entity, new SpreadComponent { Degrees = authoring.spreadDegrees });
                AddComponent(entity , new StrikerTurretCostComponent { Cost = authoring.deploymentCost });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                
                AddComponent(entity , new StrikerTurretTag());
            }
        }
    }
}