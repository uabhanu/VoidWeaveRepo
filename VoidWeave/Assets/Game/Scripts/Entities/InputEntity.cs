namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputEntity : MonoBehaviour
    {
        #region Variables
        
        [SerializeField] private uint inputDash;
        [SerializeField] private uint inputDeploy;
        [SerializeField] private uint inputDown;
        [SerializeField] private uint inputLeft;
        [SerializeField] private uint inputRight;
        [SerializeField] private uint inputTurret1;
        [SerializeField] private uint inputTurret2;
        [SerializeField] private uint inputTurret3;
        [SerializeField] private uint inputUp;
        
        [SerializeField] private Key dashKey;
        [SerializeField] private Key deployKey;
        [SerializeField] private Key downKey;
        [SerializeField] private Key leftKey;
        [SerializeField] private Key rightKey;
        [SerializeField] private Key turret1Key;
        [SerializeField] private Key turret2Key;
        [SerializeField] private Key turret3Key;
        [SerializeField] private Key upKey;

        #endregion
        
        #region Baker

        private class InputConfigBaker : Baker<InputEntity>
        {
            public override void Bake(InputEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new DashKeyComponent { Value = authoring.dashKey });
                AddComponent(entity , new DeployKeyComponent { Value = authoring.deployKey });
                AddComponent(entity , new DownKeyComponent { Value = authoring.downKey });
                AddComponent(entity , new InputDashComponent { Value = authoring.inputDash });
                AddComponent(entity , new InputDeployComponent { Value = authoring.inputDeploy });
                AddComponent(entity , new InputDownComponent { Value = authoring.inputDown });
                AddComponent(entity , new InputLeftComponent { Value = authoring.inputLeft });
                AddComponent(entity , new InputNoneComponent());
                AddComponent(entity , new InputRightComponent { Value = authoring.inputRight });
                AddComponent(entity , new InputTurret1Component { Value = authoring.inputTurret1 });
                AddComponent(entity , new InputTurret2Component { Value = authoring.inputTurret2 });
                AddComponent(entity , new InputTurret3Component { Value = authoring.inputTurret3 });
                AddComponent(entity , new InputUpComponent { Value = authoring.inputUp });
                AddComponent(entity , new LeftKeyComponent { Value = authoring.leftKey });
                AddComponent(entity , new RightKeyComponent { Value = authoring.rightKey });
                AddComponent(entity , new Turret1KeyComponent { Value = authoring.turret1Key });
                AddComponent(entity , new Turret2KeyComponent { Value = authoring.turret2Key });
                AddComponent(entity , new Turret3KeyComponent { Value = authoring.turret3Key });
                AddComponent(entity , new UpKeyComponent { Value = authoring.upKey });
            }
        }

        #endregion
    }
}