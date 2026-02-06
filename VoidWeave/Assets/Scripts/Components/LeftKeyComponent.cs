namespace Components
{
    using Unity.Entities;
    using UnityEngine.InputSystem;

    public struct LeftKeyComponent : IComponentData
    {
        public Key Value;
    }
}