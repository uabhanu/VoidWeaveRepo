namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class BeamTurretEntity : MonoBehaviour
    {
        [SerializeField] private float attackRate;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cooldownTime;
        [SerializeField] private float damage;
        [SerializeField] private int deploymentCost;
        [SerializeField] private int projectileCount;
        [SerializeField] private float range;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;

        private class BeamTurretBaker : Baker<BeamTurretEntity>
        {
            public override void Bake(BeamTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new AttackRateComponent { AttackRate = authoring.attackRate });
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new CooldownComponent { Timer = authoring.cooldownTime });
                AddComponent(entity , new DamageComponent { Damage = authoring.damage });
                AddComponent(entity , new ProjectileCountComponent { Count = authoring.projectileCount });
                AddComponent(entity , new RangeComponent { Range = authoring.range });
                AddComponent(entity , new SpreadComponent { Degrees = authoring.spreadDegrees });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                AddComponent(entity , new TurretCostComponent { Cost = authoring.deploymentCost });

                AddComponent(entity , new BeamTurretTag());
            }
        }
    }
}