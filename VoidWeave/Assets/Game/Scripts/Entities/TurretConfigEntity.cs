namespace Game.Scripts.Entities
{
    using Game.Scripts.Components;
    using Unity.Entities;
    using UnityEngine;

    public class TurretConfigEntity : MonoBehaviour
    {
        [SerializeField] private int beamTurretCost;
        [SerializeField] private GameObject beamTurretPrefab;

        [SerializeField] private int scatterTurretCost;
        [SerializeField] private GameObject scatterTurretPrefab;

        [SerializeField] private int strikerTurretCost;
        [SerializeField] private GameObject strikerTurretPrefab;

        private class TurretConfigBaker : Baker<TurretConfigEntity>
        {
            public override void Bake(TurretConfigEntity authoring)
            {
                // 1. Striker Config (On the Primary Entity)
                Entity strikerTurretEntityConfig = GetEntity(TransformUsageFlags.None);
                AddComponent(strikerTurretEntityConfig , new StrikerTurretTag());
                AddComponent(strikerTurretEntityConfig , new TurretCostComponent { Value = authoring.strikerTurretCost });
                AddComponent(strikerTurretEntityConfig , new TurretEntityComponent { Entity = GetEntity(authoring.strikerTurretPrefab , TransformUsageFlags.Dynamic) });

                // 2. Scatter Config (New Entity)
                Entity scatterTurretEntityConfig = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(scatterTurretEntityConfig , new ScatterTurretTag());
                AddComponent(scatterTurretEntityConfig , new TurretCostComponent { Value = authoring.scatterTurretCost });
                AddComponent(scatterTurretEntityConfig , new TurretEntityComponent { Entity = GetEntity(authoring.scatterTurretPrefab , TransformUsageFlags.Dynamic) });

                // 3. Beam Config (New Entity)
                Entity beamTurretEntityConfig = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(beamTurretEntityConfig , new BeamTurretTag());
                AddComponent(beamTurretEntityConfig , new TurretCostComponent { Value = authoring.beamTurretCost });
                AddComponent(beamTurretEntityConfig , new TurretEntityComponent { Entity = GetEntity(authoring.beamTurretPrefab , TransformUsageFlags.Dynamic) });
            }
        }
    }
}