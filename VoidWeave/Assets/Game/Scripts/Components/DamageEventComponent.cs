namespace Game.Scripts.Components
{
    using Unity.Entities;

    public struct DamageEventComponent : IComponentData , IEnableableComponent
    {
        public float Value;
    }
}