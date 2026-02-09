namespace Game.Scripts.Components
{
    using Unity.Entities;
    using UnityEngine.InputSystem;

    public struct RightKeyComponent : IComponentData
    {
        public Key Value;
    }
}