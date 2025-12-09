namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class EnemyEntity : MonoBehaviour
    {
        private class EnemyBaker : Baker<EnemyEntity>
        {
            public override void Bake(EnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new TurretTargetTag());
            }
        }
    }
}