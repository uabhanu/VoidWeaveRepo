namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private int scatterTurretCost;
        [SerializeField] private GameObject scatterTurretPrefab;
        [SerializeField] private int startingResources;
        [SerializeField] private int strikerTurretCost;
        [SerializeField] private GameObject strikerTurretPrefab;

        class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BaseMoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity, new CurrentEnergyComponent { Energy = authoring.startingResources });
                AddComponent(entity, new TurretDeploymentInputComponent());
                AddComponent(entity , new DashCooldownComponent());
                AddComponent(entity , new DashDurationComponent());
                AddComponent(entity , new DashInputComponent());
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new ScatterTurretEntityComponent { Entity = GetEntity(authoring.scatterTurretPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new ScatterTurretCostComponent { Cost = authoring.scatterTurretCost });
                AddComponent(entity , new ScatterTurretInputComponent());
                AddComponent(entity , new SelectedTurretCostComponent { Cost = authoring.strikerTurretCost });
                AddComponent(entity , new SelectedTurretEntityComponent { Entity = GetEntity(authoring.strikerTurretPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new StrikerTurretCostComponent { Cost = authoring.strikerTurretCost });
                AddComponent(entity , new StrikerTurretEntityComponent { Entity = GetEntity(authoring.strikerTurretPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new StrikerTurretInputComponent());
                AddComponent(entity , new TeamComponent { ID = 0 });
                
                AddComponent(entity , new PlayerTag());
                AddComponent(entity , new TargetTag());
            }
        }
    }
}