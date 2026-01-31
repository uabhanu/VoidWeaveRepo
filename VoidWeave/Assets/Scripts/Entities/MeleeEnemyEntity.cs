namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class MeleeEnemyEntity : MonoBehaviour
    {
        [SerializeField] private float attackRate;
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private int damage;
        [SerializeField] private int lootAmount;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private int maxHealth;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int teamID;
        [SerializeField] private float zigZagAmplitude;
        [SerializeField] private float zigZagFrequency;

        private class MeleeEnemyBaker : Baker<MeleeEnemyEntity>
        {
            public override void Bake(MeleeEnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new AttackRateComponent { AttackRate = authoring.attackRate });
                AddComponent(entity , new CollisionRadiusComponent { Radius = authoring.collisionRadius });
                AddComponent(entity , new CooldownComponent());
                AddComponent(entity , new CurrentHealthComponent { CurrentHealth = authoring.maxHealth });
                AddComponent(entity , new DamageComponent { Damage = authoring.damage });
                AddComponent(entity , new LootAmountComponent { Amount = authoring.lootAmount });
                AddComponent(entity , new LootEntityComponent { Entity = GetEntity(authoring.lootPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MaxHealthComponent { MaxHealth = authoring.maxHealth });
                AddComponent(entity , new MovementZigZagAmplitudeComponent { ZigZagAmplitude = authoring.zigZagAmplitude });
                AddComponent(entity , new MovementZigZagFrequencyComponent { ZigZagFrequency = authoring.zigZagFrequency });
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new RangeComponent());
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                
                AddComponent(entity , new EnemyTag());
            }
        }
    }
}