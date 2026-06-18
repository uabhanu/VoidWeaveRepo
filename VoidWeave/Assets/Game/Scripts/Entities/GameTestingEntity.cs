namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class GameTestingEntity : MonoBehaviour
    {
        [SerializeField] private int currentEnergyWhileTesting;
        [SerializeField] private int enemiesToKillWhileTesting;
        [SerializeField] private bool isTesting;
        [SerializeField] private int levelWhileTesting;
        [SerializeField] private bool muteWhileTesting;
        [SerializeField] private int timerWhileTesting;

        private class GameTesterBaker : Baker<GameTestingEntity>
        {
            public override void Bake(GameTestingEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                int isMuted = System.Convert.ToInt32(authoring.muteWhileTesting);
                int isTesting = System.Convert.ToInt32(authoring.isTesting);

                AddComponent(entity , new CurrentEnergyWhileTestingComponent { Value = authoring.currentEnergyWhileTesting });
                AddComponent(entity , new EnemiesToKillWhileTestingComponent { Value = authoring.enemiesToKillWhileTesting });
                AddComponent(entity , new IsTestingComponent { Value = isTesting });
                AddComponent(entity , new LevelWhileTestingComponent { Value = authoring.levelWhileTesting });
                AddComponent(entity , new MuteWhileTestingComponent { Value = isMuted });
                AddComponent(entity , new TimerWhileTestingComponent { Value = authoring.timerWhileTesting });
                
                AddComponent(entity , new IsTestingTag());
                SetComponentEnabled<IsTestingTag>(entity , authoring.isTesting);
            }
        }
    }
}