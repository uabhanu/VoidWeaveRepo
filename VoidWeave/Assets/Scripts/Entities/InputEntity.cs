namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputEntity : MonoBehaviour
    {
        #region Variables
        
        [Header("Input Bitmask Constants")]
        [SerializeField] private uint inputDash = 16;
        [SerializeField] private uint inputDeploy = 32;
        [SerializeField] private uint inputDown = 2;
        [SerializeField] private uint inputLeft = 4;
        [SerializeField] private uint inputRight = 8;
        [SerializeField] private uint inputTurret1 = 64;
        [SerializeField] private uint inputTurret2 = 128;
        [SerializeField] private uint inputTurret3 = 256;
        [SerializeField] private uint inputUp = 1;

        [Header("Key Bindings")]
        [SerializeField] private Key dashKey = Key.LeftShift;
        [SerializeField] private Key deployKey = Key.Space;
        [SerializeField] private Key downKey = Key.S;
        [SerializeField] private Key leftKey = Key.A;
        [SerializeField] private Key rightKey = Key.D;
        [SerializeField] private Key turret1Key = Key.Digit1;
        [SerializeField] private Key turret2Key = Key.Digit2;
        [SerializeField] private Key turret3Key = Key.Digit3;
        [SerializeField] private Key upKey = Key.W;
        
        #endregion
        
        #region Baker

        private class InputConfigBaker : Baker<InputEntity>
        {
            public override void Bake(InputEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new DashKeyComponent { DashKey = authoring.dashKey });
                AddComponent(entity , new DeployKeyComponent { DeployKey = authoring.deployKey });
                AddComponent(entity , new DownKeyComponent { DownKey = authoring.downKey });
                AddComponent(entity , new InputDashComponent { InputDash = authoring.inputDash });
                AddComponent(entity , new InputDeployComponent { InputDeployValue = authoring.inputDeploy });
                AddComponent(entity , new InputDownComponent { InputDown = authoring.inputDown });
                AddComponent(entity , new InputLeftComponent { InputLeft = authoring.inputLeft });
                AddComponent(entity , new InputNoneComponent());
                AddComponent(entity , new InputRightComponent { InputRight = authoring.inputRight });
                AddComponent(entity , new InputTurret1Component { InputTurret1Value = authoring.inputTurret1 });
                AddComponent(entity , new InputTurret2Component { InputTurret2Value = authoring.inputTurret2 });
                AddComponent(entity , new InputTurret3Component { InputTurret3Value = authoring.inputTurret3 });
                AddComponent(entity , new InputUpComponent { InputUp = authoring.inputUp });
                AddComponent(entity , new LeftKeyComponent { LeftKey = authoring.leftKey });
                AddComponent(entity , new RightKeyComponent { RightKey = authoring.rightKey });
                AddComponent(entity , new Turret1KeyComponent { Turret1Key = authoring.turret1Key });
                AddComponent(entity , new Turret2KeyComponent { Turret2Key = authoring.turret2Key });
                AddComponent(entity , new Turret3KeyComponent { Turret3Key = authoring.turret3Key });
                AddComponent(entity , new UpKeyComponent { UpKey = authoring.upKey });
            }
        }
        
        #endregion
    }
}