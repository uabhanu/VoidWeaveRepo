namespace Components
{
    using Unity.Entities;
    using UnityEngine.InputSystem;

    public struct DownKeyComponent : IComponentData
    {
        public Key Value;
    }
}