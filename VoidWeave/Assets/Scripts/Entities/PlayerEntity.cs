namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private float dashCooldownTimer; // Time before next dash   
        [SerializeField] private float dashDuration; // Length of dash   
        [SerializeField] private float dashMultiplier; // Speed boost (5 * 5 = 25 units/sec)
        [SerializeField] private int health;
        [SerializeField] private float moveSpeed;
        [SerializeField] private int scatterTurretCost;
        [SerializeField] private GameObject scatterTurretPrefab;
        [SerializeField] private int startingResources;
        [SerializeField] private int strikerTurretCost;
        [SerializeField] private GameObject strikerTurretPrefab;
        [SerializeField] private int teamID;

        class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BaseMoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new CurrentEnergyComponent { Energy = authoring.startingResources });
                AddComponent(entity , new DashCooldownComponent { Timer = authoring.dashCooldownTimer });
                AddComponent(entity , new DashDurationComponent { Duration = authoring.dashDuration });
                AddComponent(entity , new DashMultiplierComponent { Multiplier = authoring.dashMultiplier });
                AddComponent(entity , new HealthComponent { Health = authoring.health });
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new PlayerInputComponent());
                AddComponent(entity , new ScatterTurretEntityComponent { Entity = GetEntity(authoring.scatterTurretPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new ScatterTurretCostComponent { Cost = authoring.scatterTurretCost });
                AddComponent(entity , new SelectedTurretCostComponent { Cost = authoring.strikerTurretCost });
                AddComponent(entity , new SelectedTurretEntityComponent { Entity = GetEntity(authoring.strikerTurretPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new StrikerTurretCostComponent { Cost = authoring.strikerTurretCost });
                AddComponent(entity , new StrikerTurretEntityComponent { Entity = GetEntity(authoring.strikerTurretPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });

                AddComponent(entity , new PlayerTag());
            }
        }
    }
}