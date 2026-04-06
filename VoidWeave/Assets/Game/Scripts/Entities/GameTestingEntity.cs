namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class GameTestingEntity : MonoBehaviour
    {
        [SerializeField] private int currentEnergyWhileTesting;
        [SerializeField] private bool isTesting;

        private class GameTesterBaker : Baker<GameTestingEntity>
        {
            public override void Bake(GameTestingEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new CurrentEnergyWhileTestingComponent { Value = authoring.currentEnergyWhileTesting });
                AddComponent(entity , new IsTestingComponent { Value = authoring.isTesting });
            }
        }
    }
}