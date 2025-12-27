namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class RangedEnemyEntity : MonoBehaviour
    {
        [SerializeField] private int damage;
        [SerializeField] private float fireRate;
        [SerializeField] private int health;
        [SerializeField] private int lootAmount;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int projectileCount;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range;
        [SerializeField] private float spreadDegrees;
        [SerializeField] private int teamID;

        private class RangedEnemyBaker : Baker<RangedEnemyEntity>
        {
            public override void Bake(RangedEnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new BulletEntityComponent { Entity = GetEntity(authoring.projectilePrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new DamageComponent { Damage = authoring.damage });
                AddComponent(entity , new EnemyReloadTimerComponent { Timer = 0f });
                AddComponent(entity , new FireRateComponent { FireRate = authoring.fireRate });
                AddComponent(entity , new HealthComponent { Health = authoring.health });
                AddComponent(entity , new LootAmountComponent { Amount = authoring.lootAmount });
                AddComponent(entity , new LootEntityComponent { Entity = GetEntity(authoring.lootPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new ProjectileCountComponent { Count = authoring.projectileCount });
                AddComponent(entity , new RangeComponent { Range = authoring.range });
                AddComponent(entity , new SpreadComponent { Degrees = authoring.spreadDegrees });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                
                AddComponent(entity , new EnemyTag());
            }
        }
    }
}