namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class ProjectileEntity : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float lifetime;
        [SerializeField] private int teamID;

        class ProjectileBaker : Baker<ProjectileEntity>
        {
            public override void Bake(ProjectileEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.speed });
                AddComponent(entity , new ProjectileDamageComponent { Damage = 0 });
                AddComponent(entity , new ProjectileLifetimeComponent { Timer = authoring.lifetime });
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                
                AddComponent(entity , new ProjectileTag());
            }
        }
    }
}