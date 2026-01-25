namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    public class TurretConfigEntity : MonoBehaviour
    {
        [SerializeField] private GameObject beamTurretPrefab;
        [SerializeField] private GameObject scatterTurretPrefab;
        [SerializeField] private GameObject strikerTurretPrefab;

        class TurretConfigBaker : Baker<TurretConfigEntity>
        {
            public override void Bake(TurretConfigEntity authoring)
            {
                // 1. Striker Config (On the Primary Entity)
                Entity strikerTurretEntityConfig = GetEntity(TransformUsageFlags.None);
                AddComponent(strikerTurretEntityConfig , new StrikerTurretTag());
                AddComponent(strikerTurretEntityConfig , new TurretEntityComponent { Entity = GetEntity(authoring.strikerTurretPrefab , TransformUsageFlags.Dynamic) });

                // 2. Scatter Config (New Entity)
                Entity scatterTurretEntityConfig = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(scatterTurretEntityConfig , new ScatterTurretTag());
                AddComponent(scatterTurretEntityConfig , new TurretEntityComponent { Entity = GetEntity(authoring.scatterTurretPrefab , TransformUsageFlags.Dynamic) });

                // 3. Beam Config (New Entity)
                Entity beamTurretEntityConfig = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(beamTurretEntityConfig , new BeamTurretTag());
                AddComponent(beamTurretEntityConfig , new TurretEntityComponent { Entity = GetEntity(authoring.beamTurretPrefab , TransformUsageFlags.Dynamic) });
            }
        }
    }
}