namespace Game.Systems
{
    using Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [BurstCompile]
    public partial struct TurretDeploymentSystem : ISystem
    {
        public void OnUpdate(ref SystemState systemState)
        {
            if(!SystemAPI.TryGetSingleton<InputDataComponent>(out var inputDataComponent)) { return; }

            if(!inputDataComponent.DeployPressed) { return; }

            float2 cursorWorldPosition = inputDataComponent.CursorWorldPosition;

            foreach(var (localTransform , turretDeploymentComponentRef) in SystemAPI.Query<RefRO<LocalTransform> , RefRW<TurretDeploymentComponent>>().WithAll<PlayerTag>())
            {
                ref var turretDeploymentComponent = ref turretDeploymentComponentRef.ValueRW;

                if(turretDeploymentComponent.CurrentNodes >= turretDeploymentComponent.MaxNodes)
                {
                    Debug.Log("Cannot deploy: Maximum turret nodes reached");
                    continue;
                }

                float2 playerPosition = localTransform.ValueRO.Position.xy;
                float distanceToTarget = math.distance(playerPosition , cursorWorldPosition);

                if(distanceToTarget > turretDeploymentComponent.DeploymentRange)
                {
                    Debug.Log($"Cannot deploy: Target is {distanceToTarget:F2} units away (max: {turretDeploymentComponent.DeploymentRange})");
                    continue;
                }

                Debug.Log($"Turret deployment validated at {cursorWorldPosition}");
                turretDeploymentComponent.CurrentNodes++;
            }
        }
    }
}