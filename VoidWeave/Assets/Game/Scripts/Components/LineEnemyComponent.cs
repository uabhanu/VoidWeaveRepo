namespace Game.Scripts.Components
{
    using Unity.Entities;

    //This is to identify the Melee Enemy as Line Enemy
    public struct LineEnemyComponent : IComponentData
    {
        public int Value;
    }
}