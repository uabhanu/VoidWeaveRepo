namespace Game.Scripts.Entities
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

                AddComponent(entity , new CollisionRadiusComponent { Value = authoring.collisionRadius });
                AddComponent(entity , new DamageComponent());
                AddComponent(entity , new MoveSpeedComponent { Value = authoring.speed });
                AddComponent(entity , new ProjectileLifetimeComponent { Value = authoring.lifetime });
                AddComponent(entity , new TeamComponent { Value = authoring.teamID });
                AddComponent(entity , new VelocityComponent());

                AddComponent(entity , new ProjectileTag());
            }
        }
    }
}