namespace Game.Components
{
    using Unity.Entities;

    public struct TurretDeploymentComponent : IComponentData
    {
        public int CurrentNodes;
        public float DeploymentRange;
        public int MaxNodes;
    }
}