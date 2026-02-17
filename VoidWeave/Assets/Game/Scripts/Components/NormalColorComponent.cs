namespace Game.Scripts.Components
{
    using Unity.Entities;
    using UnityEngine;

    public struct NormalColorComponent : IComponentData
    {
        public Color Value;
    }
}