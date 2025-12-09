namespace Entities
{
    using Gameplay;
    using Unity.Entities;
    using UnityEngine;

    public class StrikerTurretEntity : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private float fireRate;
        [SerializeField] private float range;
        [SerializeField] private int cost;

        class StrikerBaker : Baker<StrikerTurretEntity>
        {
            public override void Bake(StrikerTurretEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new StrikerTurretTag());
                
                AddComponent(entity , new TurretCooldownComponent { Timer = authoring.fireRate });
                AddComponent(entity , new TurretDamageComponent { Damage = authoring.damage });
                AddComponent(entity , new TurretDeploymentCostComponent { Cost = authoring.cost });
                AddComponent(entity , new TurretFireRateComponent { Rate = authoring.fireRate });
                AddComponent(entity , new TurretRangeComponent { Range = authoring.range });
            }
        }
    }
}