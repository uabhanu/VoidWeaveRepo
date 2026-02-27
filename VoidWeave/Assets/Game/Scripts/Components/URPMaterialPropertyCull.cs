namespace Game.Scripts.Components
{
    using Unity.Entities;
    using Unity.Rendering;

    [MaterialProperty("_Cull")]
    public struct URPMaterialPropertyCull : IComponentData
    {
        public float Value;
    }
}