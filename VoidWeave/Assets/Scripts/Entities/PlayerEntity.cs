namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private float dashCooldownTimer; // Time before next dash   
        [SerializeField] private float dashDuration; // Length of dash   
        [SerializeField] private float dashMultiplier; // Speed boost (5 * 5 = 25 units/sec)
        [SerializeField] private int maxHealth;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int teamID;

        private class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BaseMoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new CollisionRadiusComponent { Radius = authoring.collisionRadius });
                AddComponent(entity , new CurrentHealthComponent { CurrentHealth = authoring.maxHealth });
                AddComponent(entity , new DashCooldownComponent { Timer = authoring.dashCooldownTimer });
                AddComponent(entity , new DashDurationComponent { Duration = authoring.dashDuration });
                AddComponent(entity , new DashMultiplierComponent { Multiplier = authoring.dashMultiplier });
                AddComponent(entity , new MaxHealthComponent { MaxHealth = authoring.maxHealth });
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new PlayerInputComponent());
                AddComponent(entity , new SelectedTurretCostComponent { Cost = 0 });
                AddComponent(entity , new SelectedTurretEntityComponent { Entity = Entity.Null });
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });

                AddComponent(entity , new PlayerTag());
            }
        }
    }
}