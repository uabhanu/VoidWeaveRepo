namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class ProjectileEntity : MonoBehaviour
    {
        [SerializeField] private float collisionRadius; // Defines the radius of the hitbox used for collision detection
        [SerializeField] private float lifetime;
        [SerializeField] private float speed;
        [SerializeField] private int teamID;

        private class ProjectileBaker : Baker<ProjectileEntity>
        {
            public override void Bake(ProjectileEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new CollisionRadiusComponent { Radius = authoring.collisionRadius });
                AddComponent(entity , new DamageComponent());
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.speed });
                AddComponent(entity , new ProjectileLifetimeComponent { Timer = authoring.lifetime });
                AddComponent(entity , new TeamComponent { ID = authoring.teamID });
                AddComponent(entity , new VelocityComponent());
                
                AddComponent(entity , new ProjectileTag());
            }
        }
    }
}