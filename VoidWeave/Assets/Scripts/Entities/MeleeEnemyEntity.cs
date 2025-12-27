namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class MeleeEnemyEntity : MonoBehaviour
    {
        [SerializeField] private float meleeAttackRate;
        [SerializeField] private int damage;
        [SerializeField] private int health;
        [SerializeField] private int lootAmount;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int teamID;

        private class MeleeEnemyBaker : Baker<MeleeEnemyEntity>
        {
            public override void Bake(MeleeEnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new DamageComponent { Damage = authoring.damage });
                AddComponent(entity , new HealthComponent { Health = authoring.health });
                AddComponent(entity , new LootAmountComponent { Amount = authoring.lootAmount });
                AddComponent(entity , new LootEntityComponent { Entity = GetEntity(authoring.lootPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MeleeAttackRateComponent { MeleeAttackRate = authoring.meleeAttackRate });
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                
                AddComponent(entity , new EnemyTag());
            }
        }
    }
}