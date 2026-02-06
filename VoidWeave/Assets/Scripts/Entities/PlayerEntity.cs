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

                AddComponent(entity , new BaseMoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new CollisionRadiusComponent { Value = authoring.collisionRadius });
                AddComponent(entity , new CurrentHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new DashCooldownComponent { Value = authoring.dashCooldownTimer });
                AddComponent(entity , new DashDurationComponent { Value = authoring.dashDuration });
                AddComponent(entity , new DashMultiplierComponent { Value = authoring.dashMultiplier });
                AddComponent(entity , new MaxHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new MoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new PlayerInputComponent());
                AddComponent(entity , new SelectedTurretCostComponent());
                AddComponent(entity , new SelectedTurretEntityComponent { Entity = Entity.Null });
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });

                AddComponent(entity , new PlayerTag());
            }
        }
    }
}