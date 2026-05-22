namespace Game.Scripts.Systems
{
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class GameplaySystemGroup : ComponentSystemGroup {}
}