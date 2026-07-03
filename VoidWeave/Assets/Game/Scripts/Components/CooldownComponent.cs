namespace Game.Scripts.Components
{
    using Unity.Entities;

    public struct CooldownComponent : IComponentData , IEnableableComponent
    {
        public float Value;
    }
}