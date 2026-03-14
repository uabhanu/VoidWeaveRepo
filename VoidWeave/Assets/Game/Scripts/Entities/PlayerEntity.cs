namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private float dashCooldownTimer; // Time before next dash   
        [SerializeField] private float dashDuration; // Length of dash   
        [SerializeField] private float dashMultiplier; // Speed boost (5 * 5 = 25 units/sec)
        [SerializeField] private GameObject damageVfxPrefab;
        [SerializeField] private GameObject dashVfxPrefab;
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private int maxHealth;
        [SerializeField] private GameObject movementVfxPrefab;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int teamID;
        [SerializeField] private Color vfxColor;
        [SerializeField] private float vfxSize;

        private class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BaseMoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new CollisionRadiusComponent { Value = authoring.collisionRadius });
                AddComponent(entity , new CurrentHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new DamageVfxComponent { Value = GetEntity(authoring.damageVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new DashCooldownComponent { Value = authoring.dashCooldownTimer });
                AddComponent(entity , new DashDurationComponent { Value = authoring.dashDuration });
                AddComponent(entity , new DashMultiplierComponent { Value = authoring.dashMultiplier });
                AddComponent(entity , new DashVfxComponent { Value = GetEntity(authoring.dashVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new DeathVfxComponent { Value = GetEntity(authoring.deathVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MaxHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new MovementVfxComponent { Value = GetEntity(authoring.movementVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new PlayerInputComponent());
                AddComponent(entity , new SelectedTurretCostComponent());
                AddComponent(entity , new SelectedTurretEntityComponent { Entity = Entity.Null });
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });
                AddComponent(entity , new VfxColorComponent { Value = new float3(authoring.vfxColor.r , authoring.vfxColor.g , authoring.vfxColor.b) });
                AddComponent(entity , new VfxSizeComponent { Value = authoring.vfxSize });

                AddComponent(entity , new DashVisualTag());
                AddComponent(entity , new PlayerTag());
            }
        }
    }
}