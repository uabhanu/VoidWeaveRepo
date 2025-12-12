namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private int startingResources;

        class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BaseMoveSpeedComponent { BaseSpeed = authoring.moveSpeed });
                AddComponent(entity, new CurrentEnergyComponent { Energy = authoring.startingResources });
                AddComponent(entity, new TurretDeploymentInputComponent());
                AddComponent(entity , new DashCooldownComponent());
                AddComponent(entity , new DashDurationComponent());
                AddComponent(entity , new DashInputComponent());
                AddComponent(entity , new MoveSpeedComponent { MoveSpeed = authoring.moveSpeed });
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new TeamComponent { ID = 0 });
                
                AddComponent(entity , new PlayerTag());
                AddComponent(entity , new TargetTag());
            }
        }
    }
}