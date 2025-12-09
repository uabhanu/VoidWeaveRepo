namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class ProjectileEntity : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private float speed;
        [SerializeField] private float lifetime;

        class ProjectileBaker : Baker<ProjectileEntity>
        {
            public override void Bake(ProjectileEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new MoveSpeedComponent { MoveSpeed = authoring.speed });
                AddComponent(entity , new ProjectileDamageComponent { Damage = authoring.damage });
                AddComponent(entity , new ProjectileLifetimeComponent { Timer = authoring.lifetime });
                
                AddComponent(entity , new ProjectileTag());
            }
        }
    }
}