namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class MovementVfxEntity : MonoBehaviour
    {
        [SerializeField] private float lifetime; 

        private class MovementVfxBaker : Baker<MovementVfxEntity>
        {
            public override void Bake(MovementVfxEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new LifetimeComponent { Value = authoring.lifetime });
            }
        }
    }
}