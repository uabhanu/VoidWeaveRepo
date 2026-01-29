namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputEntity : MonoBehaviour
    {
        [Header("Input Bitmask Constants")]
        [SerializeField] private uint inputDashValue = 16;
        [SerializeField] private uint inputDeployValue = 32;
        [SerializeField] private uint inputDownValue = 2;
        [SerializeField] private uint inputLeftValue = 4;
        [SerializeField] private uint inputRightValue = 8;
        [SerializeField] private uint inputTurret1Value = 64;
        [SerializeField] private uint inputTurret2Value = 128;
        [SerializeField] private uint inputTurret3Value = 256;
        [SerializeField] private uint inputUpValue = 1;

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

        private class InputConfigBaker : Baker<InputEntity>
        {
            public override void Bake(InputEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity , new DashKeyComponent { DashKey = authoring.dashKey });
                AddComponent(entity , new DeployKeyComponent { DeployKey = authoring.deployKey });
                AddComponent(entity , new DownKeyComponent { DownKey = authoring.downKey });
                AddComponent(entity , new InputDashValueComponent { InputDashValue = authoring.inputDashValue });
                AddComponent(entity , new InputDeployValueComponent { InputDeployValue = authoring.inputDeployValue });
                AddComponent(entity , new InputDownValueComponent { InputDownValue = authoring.inputDownValue });
                AddComponent(entity , new InputLeftValueComponent { InputLeftValue = authoring.inputLeftValue });
                AddComponent(entity , new InputNoneValueComponent());
                AddComponent(entity , new InputRightValueComponent { InputRightValue = authoring.inputRightValue });
                AddComponent(entity , new InputTurret1ValueComponent { InputTurret1Value = authoring.inputTurret1Value });
                AddComponent(entity , new InputTurret2ValueComponent { InputTurret2Value = authoring.inputTurret2Value });
                AddComponent(entity , new InputTurret3ValueComponent { InputTurret3Value = authoring.inputTurret3Value });
                AddComponent(entity , new InputUpValueComponent { InputUpValue = authoring.inputUpValue });
                AddComponent(entity , new LeftKeyComponent { LeftKey = authoring.leftKey });
                AddComponent(entity , new RightKeyComponent { RightKey = authoring.rightKey });
                AddComponent(entity , new Turret1KeyComponent { Turret1Key = authoring.turret1Key });
                AddComponent(entity , new Turret2KeyComponent { Turret2Key = authoring.turret2Key });
                AddComponent(entity , new Turret3KeyComponent { Turret3Key = authoring.turret3Key });
                AddComponent(entity , new UpKeyComponent { UpKey = authoring.upKey });
            }
        }
    }
}