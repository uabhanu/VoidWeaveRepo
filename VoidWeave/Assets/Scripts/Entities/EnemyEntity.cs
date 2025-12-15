using Components;

namespace Entities
{
    using Unity.Entities;
    using UnityEngine;

    public class EnemyEntity : MonoBehaviour
    {
        [SerializeField] private int lootAmount;
        [SerializeField] private GameObject lootPrefab; 
        [SerializeField] private float moveSpeed;
        
        private class EnemyBaker : Baker<EnemyEntity>
        {
            public override void Bake(EnemyEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity , new LootAmountComponent { Amount = authoring.lootAmount });
                AddComponent(entity , new LootEntityComponent { Entity = GetEntity(authoring.lootPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new MovementInputComponent());
                AddComponent(entity , new MoveSpeedComponent { Speed = authoring.moveSpeed });
                AddComponent(entity , new TargetPositionComponent());
                AddComponent(entity , new TeamComponent { ID = 1 });
                
                AddComponent(entity , new SeekerTag());
                AddComponent(entity , new TargetTag());
                AddComponent(entity , new TurretTargetTag());
            }
        }
    }
}