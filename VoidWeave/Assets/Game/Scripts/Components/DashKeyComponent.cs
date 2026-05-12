namespace Game.Scripts.Components
{
    using Unity.Entities;
    using UnityEngine.InputSystem;

    public struct DashKeyComponent : IComponentData
    {
        public Key Value;
    }
}