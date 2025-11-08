namespace Game.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct InputDataComponent : IComponentData
    {
        public float2 CursorWorldPosition;
        public bool DashPressed;
        public bool DeployPressed;
        public float2 MovementInput;
    }
}