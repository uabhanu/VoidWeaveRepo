namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class StrikerTurretEntity : MonoBehaviour
    {
        [SerializeField] private float attackRate;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cooldownTime;
        [SerializeField] private float damage;
        [SerializeField] private float minRotationRequired;
        [SerializeField] private int projectileCount;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float range;
        [SerializeField] private float rotationOffset;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;

        private class StrikerTurretBaker : Baker<StrikerTurretEntity>
        {
            public override void Bake(StrikerTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new AttackRateComponent { Value = authoring.attackRate });
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new CooldownComponent { Value = authoring.cooldownTime });
                AddComponent(entity , new DamageComponent { Value = authoring.damage });
                AddComponent(entity , new MinRotationRequiredComponent{ Value = authoring.minRotationRequired });
                AddComponent(entity , new ProjectileCountComponent { Value = authoring.projectileCount });
                AddComponent(entity , new ProjectileSpawnPointComponent { Value = (float3)authoring.projectileSpawnPoint.position - (float3)authoring.transform.position });
                AddComponent(entity , new RangeComponent { Value = authoring.range });
                AddComponent(entity , new RotationOffsetComponent { Value = authoring.rotationOffset });
                AddComponent(entity , new RotationSpeedComponent { Value = authoring.rotationSpeed });
                AddComponent(entity , new SpreadComponent { Value = authoring.spreadDegrees });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });

                AddComponent(entity , new StrikerTurretTag());
            }
        }
    }
}