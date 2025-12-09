namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        public float MoveSpeed = 5f;

        class PlayerBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity , new BaseMoveSpeedComponent { BaseSpeed = authoring.MoveSpeed });
                AddComponent(entity , new DashCooldownComponent());
                AddComponent(entity , new DashDurationComponent());
                AddComponent(entity , new DashInputComponent());
                AddComponent(entity , new MoveSpeedComponent { MoveSpeed = authoring.MoveSpeed });
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new PlayerTag());
            }
        }
    }
}