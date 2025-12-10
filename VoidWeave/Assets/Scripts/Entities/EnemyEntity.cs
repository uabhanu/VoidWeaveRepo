namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class EnemyEntity : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        
        private class EnemyBaker : Baker<EnemyEntity>
        {
            public override void Bake(EnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new MoveSpeedComponent { MoveSpeed = authoring.moveSpeed });
                
                AddComponent(entity , new SeekerTag());
                AddComponent(entity , new TargetTag());
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TurretTargetTag());
            }
        }
    }
}