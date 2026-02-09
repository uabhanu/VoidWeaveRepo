namespace Game.Scripts.Components
{
    using Unity.Entities;

    public struct TeamComponent : IComponentData
    {
        public int Value; // 0 = Player , 1 = Enemy and so on
    }
}