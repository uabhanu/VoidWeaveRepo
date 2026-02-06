namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class RangedEnemyEntity : MonoBehaviour
    {
        [SerializeField] private float attackRate;
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private int damage;
        [SerializeField] private int lootAmount;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private int maxHealth;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int projectileCount;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;
        [SerializeField] private float zigZagAmplitude;
        [SerializeField] private float zigZagFrequency;

        private class RangedEnemyBaker : Baker<RangedEnemyEntity>
        {
            public override void Bake(RangedEnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new AttackRateComponent { Value = authoring.attackRate });
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.projectilePrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new CollisionRadiusComponent { Value = authoring.collisionRadius });
                AddComponent(entity , new CooldownComponent());
                AddComponent(entity , new CurrentHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new DamageComponent { Value = authoring.damage });
                AddComponent(entity , new LootAmountComponent { Value = authoring.lootAmount });
                AddComponent(entity , new LootEntityComponent { Entity = GetEntity(authoring.lootPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MaxHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new MovementZigZagAmplitudeComponent { Value = authoring.zigZagAmplitude });
                AddComponent(entity , new MovementZigZagFrequencyComponent { Value = authoring.zigZagFrequency });
                AddComponent(entity , new MoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new ProjectileCountComponent { Value = authoring.projectileCount });
                AddComponent(entity , new RangeComponent { Value = authoring.range });
                AddComponent(entity , new SpreadComponent { Value = authoring.spreadDegrees });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });
                
                AddComponent(entity , new EnemyTag());
            }
        }
    }
}