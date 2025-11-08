namespace Game.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class PlayerEntity : MonoBehaviour
    {
        #region Variables

        [Header("Dash Settings")]
        [SerializeField] private float dashCooldown;
        [SerializeField] private float dashDistance;
        [SerializeField] private float dashDuration;

        [Header("Deployment Settings")]
        [SerializeField] private int maxTurretNodes;
        [SerializeField] private float deploymentRange;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed;

        #endregion

        #region My ECS Class

        private class PlayerEntityBaker : Baker<PlayerEntity>
        {
            public override void Bake(PlayerEntity playerEntityAuthoring)
            {
                Entity playerEntity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(playerEntity , new PlayerTag());
                AddComponent(playerEntity , new PlayerMovementComponent { CurrentVelocity = default , MoveSpeed = playerEntityAuthoring.moveSpeed });
                AddComponent(playerEntity , new DashComponent { CooldownDuration = playerEntityAuthoring.dashCooldown , CurrentCooldown = 0f , DashDirection = default , DashDistance = playerEntityAuthoring.dashDistance , DashDuration = playerEntityAuthoring.dashDuration , IsDashing = false , DashTimer = 0f });
                AddComponent(playerEntity , new TurretDeploymentComponent { CurrentNodes = 0 , DeploymentRange = playerEntityAuthoring.deploymentRange , MaxNodes = playerEntityAuthoring.maxTurretNodes });
            }
        }

        #endregion
    }
}