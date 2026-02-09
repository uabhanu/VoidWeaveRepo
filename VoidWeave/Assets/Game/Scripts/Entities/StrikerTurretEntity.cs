namespace Game.Scripts.Entities
{
    using Game.Scripts.Components;
    using Unity.Entities;
    using UnityEngine;

    public class StrikerTurretEntity : MonoBehaviour
    {
        [SerializeField] private float attackRate;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cooldownTime;
        [SerializeField] private float damage;
        [SerializeField] private int projectileCount;
        [SerializeField] private float range;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;

        private class StrikerBaker : Baker<StrikerTurretEntity>
        {
            public override void Bake(StrikerTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new AttackRateComponent { Value = authoring.attackRate });
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new CooldownComponent { Value = authoring.cooldownTime });
                AddComponent(entity , new DamageComponent { Value = authoring.damage });
                AddComponent(entity , new ProjectileCountComponent { Value = authoring.projectileCount });
                AddComponent(entity , new RangeComponent { Value = authoring.range });
                AddComponent(entity , new SpreadComponent { Value = authoring.spreadDegrees });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });

                AddComponent(entity , new StrikerTurretTag());
            }
        }
    }
}