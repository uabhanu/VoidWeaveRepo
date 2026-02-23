namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class BeamTurretEntity : MonoBehaviour
    {
        [SerializeField] private float attackRate;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float cooldownTime;
        [SerializeField] private float damage;
        [SerializeField] private float minRotationRequired;
        [SerializeField] private int projectileCount;
        [SerializeField] private float range;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;

        private class BeamTurretBaker : Baker<BeamTurretEntity>
        {
            public override void Bake(BeamTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new AttackRateComponent { Value = authoring.attackRate });
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.bulletPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new CooldownComponent { Value = authoring.cooldownTime });
                AddComponent(entity , new DamageComponent { Value = authoring.damage });
                AddComponent(entity , new MinRotationRequiredComponent { Value = authoring.minRotationRequired });
                AddComponent(entity , new NozzleOffsetComponent { Value = (float3)authoring.spawnPoint.position - (float3)authoring.transform.position });
                AddComponent(entity , new ProjectileCountComponent { Value = authoring.projectileCount });
                AddComponent(entity , new RangeComponent { Value = authoring.range });
                AddComponent(entity , new SpreadComponent { Value = authoring.spreadDegrees });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });

                AddComponent(entity , new BeamTurretTag());
            }
        }
    }
}