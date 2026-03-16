namespace Game.Scripts.Entities
{
    using Unity.Entities;
    using UnityEngine;

    public class GameBackgroundEntity : MonoBehaviour
    {
        private class GameBackgroundBaker : Baker<GameBackgroundEntity>
        {
            public override void Bake(GameBackgroundEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }
}