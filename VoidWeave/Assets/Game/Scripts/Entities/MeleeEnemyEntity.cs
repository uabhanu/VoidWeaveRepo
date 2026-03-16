namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class MeleeEnemyEntity : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float attackRate;
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private int damage;
        [SerializeField] private GameObject damageVfxPrefab;
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private int isLineEnemy;
        [SerializeField] private int isTriangleEnemy;
        [SerializeField] private int lootAmount;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private int maxHealth;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float spawnVfxDuration;
        [SerializeField] private int teamID;
        [SerializeField] private Color vfxColor;
        [SerializeField] private float vfxSize;
        [SerializeField] private float zigZagAmplitude;
        [SerializeField] private float zigZagFrequency;

        #endregion

        #region Baker

        private class MeleeEnemyBaker : Baker<MeleeEnemyEntity>
        {
            public override void Bake(MeleeEnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                var bodyVisual = authoring.transform.Find("BodyVisual");
                float3 bodyVisualScale = bodyVisual ? bodyVisual.localScale : (float3)authoring.transform.localScale;
                var meshFilter = authoring.GetComponentInChildren<MeshFilter>();
                var meshRenderer = authoring.GetComponentInChildren<MeshRenderer>();
                Texture2D mainTexture = meshRenderer.sharedMaterial.mainTexture as Texture2D;

                AddComponent(entity , new AttackRateComponent { Value = authoring.attackRate });
                AddComponent(entity , new CollisionRadiusComponent { Value = authoring.collisionRadius });
                AddComponent(entity , new CooldownComponent());
                AddComponent(entity , new CurrentHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new DamageComponent { Value = authoring.damage });
                AddComponent(entity , new DamageVfxComponent { Value = GetEntity(authoring.damageVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new DeathVfxComponent { Value = GetEntity(authoring.deathVfxPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LineEnemyComponent { Value = authoring.isLineEnemy });
                AddComponent(entity , new LootAmountComponent { Value = authoring.lootAmount });
                AddComponent(entity , new LootEntityComponent { Entity = GetEntity(authoring.lootPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MaxHealthComponent { Value = authoring.maxHealth });
                AddComponent(entity , new MovementZigZagAmplitudeComponent { Value = authoring.zigZagAmplitude });
                AddComponent(entity , new MovementZigZagFrequencyComponent { Value = authoring.zigZagFrequency });
                AddComponent(entity , new MoveSpeedComponent { Value = authoring.moveSpeed });
                AddComponent(entity , new RangeComponent());
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });
                AddComponent(entity , new TimerComponent { Value = authoring.spawnVfxDuration });
                AddComponent(entity , new TriangleEnemyComponent { Value = authoring.isTriangleEnemy });
                AddComponent(entity , new VfxColorComponent { Value = new float3(authoring.vfxColor.r , authoring.vfxColor.g , authoring.vfxColor.b) });
                AddComponent(entity , new VfxScaleComponent { Value = bodyVisualScale });
                AddComponent(entity , new VfxSizeComponent { Value = authoring.vfxSize });

                AddComponentObject(entity , new VfxMeshComponent { Value = meshFilter.sharedMesh });
                AddComponentObject(entity , new VfxTextureComponent { Value = mainTexture });

                AddComponent(entity , new EnemyTag());
            }
        }

        #endregion
    }
}