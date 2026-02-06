namespace Components
{
    using Unity.Entities;
    using UnityEngine.InputSystem;

    public struct UpKeyComponent : IComponentData
    {
        public Key Value;
    }
}