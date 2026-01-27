namespace Entities
{
    using Components;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputEntity : MonoBehaviour
    {
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
                AddComponent(entity , new InputDashValueComponent { InputDashValue = 16 });
                AddComponent(entity , new InputDeployValueComponent { InputDeployValue = 32 });
                AddComponent(entity , new InputDownValueComponent { InputDownValue = 2 });
                AddComponent(entity , new InputLeftValueComponent { InputLeftValue = 4 });
                AddComponent(entity , new InputNoneValueComponent { InputNoneValue = 0 });
                AddComponent(entity , new InputRightValueComponent { InputRightValue = 8 });
                AddComponent(entity , new InputTurret1ValueComponent { InputTurret1Value = 64 });
                AddComponent(entity , new InputTurret2ValueComponent { InputTurret2Value = 128 });
                AddComponent(entity , new InputTurret3ValueComponent { InputTurret3Value = 256 });
                AddComponent(entity , new InputUpValueComponent { InputUpValue = 1 });
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