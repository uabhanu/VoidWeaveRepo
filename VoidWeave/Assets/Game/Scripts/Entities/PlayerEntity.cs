namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        #region Variables
        
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private float dashCooldownDefault; // This is the value that is constant and used to set the value of the timer
        [SerializeField] private float dashDurationDefault;
        [SerializeField] private float dashMultiplier; // Speed boost (5 * 5 = 25 units/sec)
        [SerializeField] private GameObject damageVfxPrefab;
        [SerializeField] private GameObject dashVfxPrefab;
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private int maxHealth;
        [SerializeField] private GameObject movementVfxPrefab;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationOffset;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private int teamID;
        [SerializeField] private Color vfxColor;
        [SerializeField] private float vfxSize;
        
        #endregion
        
        #region Baker

        private class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                var bodyVisual = authoring.transform.Find("BodyVisual");
                float3 bodyVisualScale = bodyVisual ? bodyVisual.localScale : (float3)authoring.transform.localScale;
                var meshFilter = authoring.GetComponentInChildren<MeshFilter>();
                var meshRenderer = authoring.GetComponentInChildren<MeshRenderer>();
                Texture2D mainTexture = meshRenderer.sharedMaterial.mainTexture as Texture2D;

                AddComponent(entity , new BaseMoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new CollisionRadiusComponent { Value = authoring.collisionRadius });
                AddComponent(entity , new CurrentHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new DamageVfxComponent { Value = GetEntity(authoring.damageVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new DashCooldownComponent { Value = authoring.dashCooldownDefault });
                AddComponent(entity , new DashCooldownDefaultComponent { Value = authoring.dashCooldownDefault });
                AddComponent(entity , new DashDurationComponent { Value = authoring.dashDurationDefault });
                AddComponent(entity , new DashDurationDefaultComponent { Value = authoring.dashDurationDefault });
                AddComponent(entity , new DashMultiplierComponent { Value = authoring.dashMultiplier });
                AddComponent(entity , new DashVfxComponent { Value = GetEntity(authoring.dashVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new DeathVfxComponent { Value = GetEntity(authoring.deathVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LastSpawnPositionComponent { Value = new float3(float.MaxValue) });
                AddComponent(entity , new MaxHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new MoveDirectionComponent());
                AddComponent(entity , new MovementVfxEntityComponent { Value = GetEntity(authoring.movementVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new PlayerInputComponent());
                AddComponent(entity , new RotationOffsetComponent { Value = authoring.rotationOffset });
                AddComponent(entity , new RotationSpeedComponent { Value = authoring.rotationSpeed });
                AddComponent(entity , new SelectedTurretCostComponent());
                AddComponent(entity , new SelectedTurretEntityComponent { Entity = Entity.Null });
                AddComponent(entity , new SelectedTurretIndexComponent());
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });
                AddComponent(entity , new VfxColorComponent { Value = new float3(authoring.vfxColor.r , authoring.vfxColor.g , authoring.vfxColor.b) });
                AddComponent(entity , new VfxScaleComponent { Value = bodyVisualScale });
                AddComponent(entity , new VfxSizeComponent { Value = authoring.vfxSize });

                AddComponentObject(entity , new VfxMeshComponent { Value = meshFilter.sharedMesh });
                AddComponentObject(entity , new VfxTextureComponent { Value = mainTexture });

                AddComponent(entity , new DashVisualTag());
                AddComponent(entity , new PlayerTag());
                AddComponent(entity , new ScaleStatsTag());
            }
        }
        
        #endregion
    }
}