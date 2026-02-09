namespace Game.Scripts.Components
{
    using Unity.Entities;
    using UnityEngine.InputSystem;

    public struct DeployKeyComponent : IComponentData
    {
        public Key Value;
    }
}